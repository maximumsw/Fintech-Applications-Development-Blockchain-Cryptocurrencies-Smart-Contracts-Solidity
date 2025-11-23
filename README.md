# Fintech Projects - Blockchain Educational Monorepo

A collection of progressive .NET blockchain implementations demonstrating core concepts from basic hashing to smart contracts and advanced economic mechanisms. Built with **ASP.NET Core MVC** on **.NET 8.0**.

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

### 6. Halving
**Minimal Blockchain with Dynamic Difficulty**
- Proof-of-Work mining with configurable difficulty (1-6)
- Automatic difficulty adjustment based on block time
- Transaction management with fees and notes
- Mempool for pending transactions
- Coinbase mining rewards
- Real-time balance tracking
- Expandable transaction viewer
- Blockchain validation

**Quick Start:**
```bash
cd Halving/Halving
dotnet run
```
Navigate to `http://localhost:5298` or `https://localhost:7125`

**Key Features:**
- Target block time: 10 seconds
- Adjustment window: 5 blocks
- Mining reward: 50 coins
- Dynamic difficulty (1-10 range)

---

### 7. Halving2
**Blockchain with Halving Mechanism**
- Bitcoin-style halving (reward reduction every N blocks)
- Transaction confirmation system
- Wallet history with confirmed/pending status
- Block details with confirmation counts
- Proof-of-Work mining
- Genesis block with initial distribution (Alice, Bob, Charlie: 1000 coins each)

**Quick Start:**
```bash
cd Halving2/Halving2
dotnet run
```
Navigate to `http://localhost:5167` or `https://localhost:7115`

**Halving Schedule:**
- Base reward: 50 coins
- Halving interval: 10 blocks
- Blocks 1-10: 50 coins
- Blocks 11-20: 25 coins
- Blocks 21-30: 12.5 coins
- Formula: `reward = BaseReward / 2^(blockHeight / HalvingInterval)`

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
6. **Halving** → Economic mechanisms (dynamic difficulty, adaptive consensus)
7. **Halving2** → Tokenomics (deflationary models, confirmation systems)

## Key Features Across Projects

- **Cryptography:** SHA-256 hashing, RSA-2048 digital signatures
- **Consensus:** Proof-of-Work with configurable difficulty
- **Transactions:** Digital signature validation, balance tracking, confirmation system
- **Wallets:** RSA key pair generation, address derivation, transaction history
- **Smart Contracts:** Interface-based contract system with state management
- **Multi-Node:** Simulated 3-node network with block broadcasting
- **Economic Models:** Dynamic difficulty adjustment, halving mechanism, deflationary tokenomics
- **Advanced Features:** Mempool management, coinbase rewards, transaction fees

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

## Author

Maksym Batashan
