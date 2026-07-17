// Post-quantum primitives for the platform (SR-1, SR-3), wrapping liboqs.
//
// - KeyEncapsulation: ML-KEM-768 (FIPS 203). Two parties derive a shared secret over
//   an untrusted network; that secret becomes the ephemeral session key protecting
//   chat traffic against "harvest now, decrypt later" adversaries.
// - Signer: ML-DSA-65 (FIPS 204). Every audio/text packet is signed so MITM tampering
//   on public networks is detectable without revealing sender identity — signature
//   keys are per-session and ephemeral, never tied to a real-world identity (SR-2).
//
// This library is consumed two ways: linked directly by native code, and (planned)
// through a JNI bridge from spring-backend, replacing its interim AES-only
// EncryptionService. The API therefore sticks to plain byte vectors — no liboqs
// types leak out of this header.
#pragma once

#include <cstdint>
#include <stdexcept>
#include <string>
#include <vector>

namespace securelearning::pqc {

using Bytes = std::vector<std::uint8_t>;

class CryptoError : public std::runtime_error {
public:
    explicit CryptoError(const std::string& what) : std::runtime_error(what) {}
};

struct KemKeyPair {
    Bytes publicKey;
    Bytes secretKey;
};

struct Encapsulation {
    Bytes ciphertext;    // send this to the key-pair owner
    Bytes sharedSecret;  // keep this; the owner derives the same one via decapsulate()
};

/// ML-KEM-768 key encapsulation. All methods throw CryptoError on failure.
class KeyEncapsulation {
public:
    static KemKeyPair generateKeyPair();
    static Encapsulation encapsulate(const Bytes& publicKey);
    static Bytes decapsulate(const Bytes& ciphertext, const Bytes& secretKey);
};

struct SigKeyPair {
    Bytes publicKey;
    Bytes secretKey;
};

/// ML-DSA-65 signatures. sign() throws CryptoError; verify() returns false rather
/// than throwing, since an invalid signature is an expected runtime condition, not a bug.
class Signer {
public:
    static SigKeyPair generateKeyPair();
    static Bytes sign(const Bytes& message, const Bytes& secretKey);
    static bool verify(const Bytes& message, const Bytes& signature, const Bytes& publicKey);
};

}  // namespace securelearning::pqc
