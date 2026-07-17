#include "securelearning/pqc.hpp"

#include <memory>

#include <oqs/oqs.h>

namespace securelearning::pqc {

namespace {

constexpr const char* kKemAlg = OQS_KEM_alg_ml_kem_768;
constexpr const char* kSigAlg = OQS_SIG_alg_ml_dsa_65;

struct KemDeleter {
    void operator()(OQS_KEM* kem) const { OQS_KEM_free(kem); }
};
struct SigDeleter {
    void operator()(OQS_SIG* sig) const { OQS_SIG_free(sig); }
};

std::unique_ptr<OQS_KEM, KemDeleter> newKem() {
    std::unique_ptr<OQS_KEM, KemDeleter> kem(OQS_KEM_new(kKemAlg));
    if (!kem) {
        throw CryptoError("ML-KEM-768 is not available in this liboqs build");
    }
    return kem;
}

std::unique_ptr<OQS_SIG, SigDeleter> newSig() {
    std::unique_ptr<OQS_SIG, SigDeleter> sig(OQS_SIG_new(kSigAlg));
    if (!sig) {
        throw CryptoError("ML-DSA-65 is not available in this liboqs build");
    }
    return sig;
}

}  // namespace

KemKeyPair KeyEncapsulation::generateKeyPair() {
    auto kem = newKem();
    KemKeyPair pair;
    pair.publicKey.resize(kem->length_public_key);
    pair.secretKey.resize(kem->length_secret_key);
    if (OQS_KEM_keypair(kem.get(), pair.publicKey.data(), pair.secretKey.data()) != OQS_SUCCESS) {
        throw CryptoError("ML-KEM-768 key generation failed");
    }
    return pair;
}

Encapsulation KeyEncapsulation::encapsulate(const Bytes& publicKey) {
    auto kem = newKem();
    if (publicKey.size() != kem->length_public_key) {
        throw CryptoError("ML-KEM-768 public key has the wrong length");
    }
    Encapsulation result;
    result.ciphertext.resize(kem->length_ciphertext);
    result.sharedSecret.resize(kem->length_shared_secret);
    if (OQS_KEM_encaps(kem.get(), result.ciphertext.data(), result.sharedSecret.data(),
                       publicKey.data()) != OQS_SUCCESS) {
        throw CryptoError("ML-KEM-768 encapsulation failed");
    }
    return result;
}

Bytes KeyEncapsulation::decapsulate(const Bytes& ciphertext, const Bytes& secretKey) {
    auto kem = newKem();
    if (ciphertext.size() != kem->length_ciphertext) {
        throw CryptoError("ML-KEM-768 ciphertext has the wrong length");
    }
    if (secretKey.size() != kem->length_secret_key) {
        throw CryptoError("ML-KEM-768 secret key has the wrong length");
    }
    Bytes sharedSecret(kem->length_shared_secret);
    if (OQS_KEM_decaps(kem.get(), sharedSecret.data(), ciphertext.data(),
                       secretKey.data()) != OQS_SUCCESS) {
        throw CryptoError("ML-KEM-768 decapsulation failed");
    }
    return sharedSecret;
}

SigKeyPair Signer::generateKeyPair() {
    auto sig = newSig();
    SigKeyPair pair;
    pair.publicKey.resize(sig->length_public_key);
    pair.secretKey.resize(sig->length_secret_key);
    if (OQS_SIG_keypair(sig.get(), pair.publicKey.data(), pair.secretKey.data()) != OQS_SUCCESS) {
        throw CryptoError("ML-DSA-65 key generation failed");
    }
    return pair;
}

Bytes Signer::sign(const Bytes& message, const Bytes& secretKey) {
    auto sig = newSig();
    if (secretKey.size() != sig->length_secret_key) {
        throw CryptoError("ML-DSA-65 secret key has the wrong length");
    }
    Bytes signature(sig->length_signature);
    std::size_t signatureLength = signature.size();
    if (OQS_SIG_sign(sig.get(), signature.data(), &signatureLength, message.data(),
                     message.size(), secretKey.data()) != OQS_SUCCESS) {
        throw CryptoError("ML-DSA-65 signing failed");
    }
    signature.resize(signatureLength);
    return signature;
}

bool Signer::verify(const Bytes& message, const Bytes& signature, const Bytes& publicKey) {
    auto sig = newSig();
    if (publicKey.size() != sig->length_public_key) {
        return false;
    }
    return OQS_SIG_verify(sig.get(), message.data(), message.size(), signature.data(),
                          signature.size(), publicKey.data()) == OQS_SUCCESS;
}

}  // namespace securelearning::pqc
