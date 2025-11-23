# Fintech Projects - Blockchain Educational Monorepo

A collection of progressive .NET blockchain implementations demonstrating core concepts from basic hashing to smart contracts. Built with **ASP.NET Core MVC** on **.NET 8.0**.

## Projects

### 1. BlockchainMvc
**Basic Blockchain Implementation**
- SHA-256 hashing and chain validation
- JSON file persistence
- Block search functionality

**Quick Start:**
```bash
cd BlockchainMvc/BlockchainMvc
dotnet run
```

---

### 2. BlockchainMvc_V2
**Digital Signatures**
- RSA-2048 block signing and verification
- Tampering detection
- Key pair generation

**Quick Start:**
```bash
cd BlockchainMvc_V2/BlockchainMvc_V2
dotnet run
```

---

### 3. POW-MemPOOL
**Proof of Work Mining**
- Configurable difficulty (1-6)
- Synchronous and asynchronous mining
- Real-time progress tracking with cancellation support

**Quick Start:**
```bash
cd POW-MemPOOL/POW-MemPOOL
dotnet run
```
Navigate to `https://localhost:7123`

---

### 4. Send_Block-Сustom_BlockChain_Logic
**Full-Featured Blockchain**
- RSA-2048 wallets with address derivation
- Transaction system with digital signatures
- Mining rewards (50 coins per block)
- 3-node network simulation with consensus
- Block broadcasting and validation

**Quick Start:**
```bash
cd Send_Block-Сustom_BlockChain_Logic/Send_Block-Сustom_BlockChain_Logic
dotnet run
```
Navigate to `https://localhost:5001`

**Demo Workflow:**
1. Click "Demo Setup" to create wallets (Alice, Bob, Charlie)
2. Mine empty block to earn first coins
3. Create transactions between wallets
4. Mine transaction blocks
5. Broadcast blocks to other nodes

---

### 5. Exam
**Smart Contracts**
- Smart contract interface implementation
- Penalty-based staking system
  - Minimum lock: 20 blocks
  - Early withdrawal penalty: 20%
  - Reward rate: 0.001 per token per block
- Transaction validation through contracts

**Quick Start:**
```bash
cd Exam/Exam
dotnet run
```

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher

## Common Commands

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Clean build artifacts
dotnet clean
```

## Learning Path

1. **BlockchainMvc** → Fundamentals (hashing, chain structure)
2. **BlockchainMvc_V2** → Security (digital signatures)
3. **POW-MemPOOL** → Consensus (proof-of-work, async patterns)
4. **Send_Block** → Production features (transactions, wallets, multi-node consensus)
5. **Exam** → Advanced concepts (smart contracts, DeFi)

## Key Features Across Projects

- **Cryptography:** SHA-256 hashing, RSA-2048 digital signatures
- **Consensus:** Proof-of-Work with configurable difficulty
- **Transactions:** Digital signature validation, balance tracking
- **Wallets:** RSA key pair generation, address derivation
- **Smart Contracts:** Interface-based contract system with state management
- **Multi-Node:** Simulated 3-node network with block broadcasting

## Architecture

All projects follow **ASP.NET Core MVC** patterns:
- **Controllers:** HTTP request handling
- **Models:** Domain entities (Block, Transaction, Wallet)
- **Services:** Business logic (registered as Singletons)
- **Views:** Razor templates with Bootstrap UI

## Limitations

- **Educational purposes only** - not production-ready
- **In-memory storage** (except BlockchainMvc with JSON persistence)
- **Single-machine simulation** for multi-node features
- **No network protocols** - nodes communicate via method calls
- **Unencrypted private keys** stored in memory

## Documentation

See [CLAUDE.md](./CLAUDE.md) for detailed architecture, development patterns, and troubleshooting guides.

## Author

Maksym Batashan
