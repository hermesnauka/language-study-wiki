// Assert-based tests run by CTest (see CMakeLists.txt). Deliberately dependency-free —
// no test framework to fetch — matching the repo-wide "no live network in tests"
// convention (liboqs itself is fetched at configure time, not test time).
#include "securelearning/pqc.hpp"

#include <cstdio>
#include <cstdlib>
#include <string>

using securelearning::pqc::Bytes;
using securelearning::pqc::KeyEncapsulation;
using securelearning::pqc::Signer;

namespace {

int failures = 0;

void check(bool condition, const char* label) {
    if (condition) {
        std::printf("PASS  %s\n", label);
    } else {
        std::printf("FAIL  %s\n", label);
        ++failures;
    }
}

Bytes toBytes(const std::string& text) {
    return Bytes(text.begin(), text.end());
}

void kemRoundTrip() {
    auto pair = KeyEncapsulation::generateKeyPair();
    auto encapsulation = KeyEncapsulation::encapsulate(pair.publicKey);
    Bytes recovered = KeyEncapsulation::decapsulate(encapsulation.ciphertext, pair.secretKey);

    check(recovered == encapsulation.sharedSecret,
          "KEM: both parties derive the same shared secret");
    check(!encapsulation.sharedSecret.empty(), "KEM: shared secret is non-empty");
}

void kemTamperedCiphertextYieldsDifferentSecret() {
    // FIPS 203 implicit rejection: decapsulating a corrupted ciphertext must not
    // error out (that would leak an oracle) — it silently returns a different,
    // pseudorandom secret, so subsequent AEAD decryption simply fails.
    auto pair = KeyEncapsulation::generateKeyPair();
    auto encapsulation = KeyEncapsulation::encapsulate(pair.publicKey);
    Bytes tampered = encapsulation.ciphertext;
    tampered[0] ^= 0xFF;

    Bytes recovered = KeyEncapsulation::decapsulate(tampered, pair.secretKey);

    check(recovered != encapsulation.sharedSecret,
          "KEM: tampered ciphertext yields a different secret (implicit rejection)");
}

void kemDistinctEncapsulationsDiffer() {
    auto pair = KeyEncapsulation::generateKeyPair();
    auto first = KeyEncapsulation::encapsulate(pair.publicKey);
    auto second = KeyEncapsulation::encapsulate(pair.publicKey);

    check(first.sharedSecret != second.sharedSecret,
          "KEM: every encapsulation derives a fresh secret");
}

void signatureRoundTrip() {
    auto pair = Signer::generateKeyPair();
    Bytes message = toBytes("packet: hello from user A");
    Bytes signature = Signer::sign(message, pair.secretKey);

    check(Signer::verify(message, signature, pair.publicKey),
          "SIG: valid signature verifies");
}

void signatureRejectsTampering() {
    auto pair = Signer::generateKeyPair();
    Bytes message = toBytes("packet: original");
    Bytes signature = Signer::sign(message, pair.secretKey);

    Bytes alteredMessage = toBytes("packet: altered!");
    check(!Signer::verify(alteredMessage, signature, pair.publicKey),
          "SIG: tampered message is rejected");

    Bytes alteredSignature = signature;
    alteredSignature[0] ^= 0xFF;
    check(!Signer::verify(message, alteredSignature, pair.publicKey),
          "SIG: tampered signature is rejected");

    auto otherPair = Signer::generateKeyPair();
    check(!Signer::verify(message, signature, otherPair.publicKey),
          "SIG: wrong public key is rejected");
}

void wrongLengthInputsThrow() {
    bool threw = false;
    try {
        KeyEncapsulation::encapsulate(Bytes{1, 2, 3});
    } catch (const securelearning::pqc::CryptoError&) {
        threw = true;
    }
    check(threw, "KEM: wrong-length public key throws CryptoError");
}

}  // namespace

int main() {
    kemRoundTrip();
    kemTamperedCiphertextYieldsDifferentSecret();
    kemDistinctEncapsulationsDiffer();
    signatureRoundTrip();
    signatureRejectsTampering();
    wrongLengthInputsThrow();

    if (failures != 0) {
        std::printf("\n%d check(s) FAILED\n", failures);
        return EXIT_FAILURE;
    }
    std::printf("\nAll checks passed\n");
    return EXIT_SUCCESS;
}
