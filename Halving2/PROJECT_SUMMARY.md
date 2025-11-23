# Educational Blockchain with Halving - Project Summary

## Project Overview

This is a complete educational blockchain web application built with ASP.NET Core 8.0 MVC that demonstrates key blockchain concepts including Proof-of-Work, transaction management, halving mechanics, and block confirmations.

## Features Implemented

### Core Blockchain Functionality

1. **Block Structure**
   - Height/index tracking
   - Timestamp
   - Previous hash linking
   - Current block hash
   - Nonce for Proof-of-Work
   - Configurable difficulty
   - List of transactions
   - Mining duration tracking

2. **Transaction Model**
   - From/To addresses
   - Amount and fee
   - Optional notes
   - Block height reference (when confirmed)

3. **Blockchain Service** (`Services/BlockchainService.cs`)
   - Chain management (list of blocks)
   - Mempool (pending transactions)
   - Balance tracking
   - Transaction validation
   - Block mining with PoW
   - Halving logic implementation

### Task 1: Halving Implementation

**Location**: `Services/BlockchainService.cs` - `GetBlockReward()` method

The halving mechanism reduces block rewards over time:

- **Formula**: `reward = BaseReward / 2^k`
  - `k` = number of halvings = `blockHeight / HalvingInterval`
- **Default Settings**:
  - Base Reward: 50 coins
  - Halving Interval: 10 blocks

**Reward Schedule** (with defaults):
- Blocks 1-10: 50 coins
- Blocks 11-20: 25 coins
- Blocks 21-30: 12.5 coins
- And so on...

**Implementation Details**:
- Coinbase transaction includes: `blockReward + totalFees`
- Genesis block (height 0) has no mining reward
- Detailed explanation in `HALVING_EXPLANATION.md`

### Task 2: Block Details with Confirmations

**Location**: `Views/Blockchain/BlockDetails.cshtml`

When clicking on any block in the blockchain list:
- Displays all transactions in that block
- Shows confirmation count for each transaction
- Formula: `Confirmations = LastBlockHeight - BlockHeight + 1`
- Highlights coinbase transactions
- Links to wallet histories

### Task 3: Wallet History with Confirmations

**Location**: `Views/Blockchain/WalletHistory.cshtml`

When clicking on any wallet address:
- Shows all transactions involving that wallet
- Displays direction: Incoming/Outgoing
- Shows status: Pending (mempool) or Confirmed (in block)
- Displays confirmations using same formula as Task 2
- Shows confirmed balance vs. total balance (including pending)
- Pending transactions show "0 confirmations"
- Confirmed transactions show increasing confirmations as new blocks are mined

## User Interface

### Navigation Pages

1. **Blocks** (`/Blockchain/Index`)
   - Lists all blocks in the chain
   - Shows block stats (height, hash, timestamp, tx count, mining time)
   - Click on any block to view details

2. **Block Details** (`/Blockchain/BlockDetails/{height}`)
   - Complete block information
   - All transactions with confirmations
   - Hash and previous hash display

3. **Wallets** (`/Blockchain/Wallets`)
   - Lists all wallet addresses with balances
   - Shows confirmed vs. total balance
   - Click on address to view history

4. **Wallet History** (`/Blockchain/WalletHistory/{address}`)
   - Complete transaction history for a wallet
   - Incoming/outgoing indicator
   - Pending vs. confirmed status
   - Confirmation counts
   - Balance summary

5. **Create Transaction** (`/Blockchain/CreateTransaction`)
   - Form to create new transactions
   - Select from/to addresses
   - Set amount and fee
   - Add optional note
   - Transactions go to mempool

6. **Mine Block** (`/Blockchain/Mine`)
   - Shows pending transactions count
   - Displays next block reward (demonstrates halving)
   - Select miner address
   - Mines all pending transactions into a new block

7. **Mempool** (`/Blockchain/Mempool`)
   - View all pending transactions
   - Shows total fees
   - Quick link to mine

## Project Structure

```
Halving2/
├── Controllers/
│   ├── HomeController.cs (original)
│   └── BlockchainController.cs (all blockchain actions)
├── Models/
│   ├── Block.cs
│   ├── Transaction.cs
│   └── ErrorViewModel.cs
├── ViewModels/
│   ├── BlockDetailsViewModel.cs
│   ├── WalletViewModel.cs
│   ├── WalletHistoryViewModel.cs
│   └── WalletTransactionViewModel.cs
├── Services/
│   └── BlockchainService.cs (core logic)
├── Views/
│   ├── Blockchain/
│   │   ├── Index.cshtml (blocks list)
│   │   ├── BlockDetails.cshtml (Task 2)
│   │   ├── Wallets.cshtml
│   │   ├── WalletHistory.cshtml (Task 3)
│   │   ├── CreateTransaction.cshtml
│   │   ├── Mine.cshtml
│   │   └── Mempool.cshtml
│   └── Shared/
│       └── _Layout.cshtml (updated navigation)
└── Program.cs (service registration, routing)
```

## Running the Application

```bash
# Build
dotnet build Halving2.sln

# Run
cd Halving2
dotnet run

# Or run with specific profile
dotnet run --launch-profile http   # HTTP only at localhost:5167
dotnet run --launch-profile https  # HTTPS + HTTP
```

The application will be available at:
- HTTP: http://localhost:5167
- HTTPS: https://localhost:7115

## Testing the Features

### To Test Halving:

1. Open the application in a browser
2. Navigate to "Mine" page - note the current block reward
3. Create some transactions
4. Mine blocks repeatedly
5. After 10 blocks, observe the reward drop from 50 to 25 coins
6. Continue mining to see further halvings

### To Test Confirmations (Task 2):

1. Click on any block in the Blocks list
2. View its transactions
3. Note the confirmations count
4. Mine a new block
5. Return to the same block - confirmations should have increased by 1

### To Test Wallet History (Task 3):

1. Go to "Wallets" page
2. Click on any address (e.g., "Alice")
3. View complete transaction history
4. Observe:
   - Incoming transactions (green, positive amounts)
   - Outgoing transactions (red, negative amounts)
   - Confirmed vs. pending status
   - Confirmation counts
   - Confirmed vs. total balance

## Genesis Block

The blockchain initializes with a genesis block (height 0) containing:
- 1000 coins to Alice
- 1000 coins to Bob
- 1000 coins to Charlie

This provides initial liquidity for testing transactions.

## Key Implementation Highlights

1. **Proof-of-Work**: Blocks must have a hash starting with N zeros (configurable difficulty)
2. **Immutable Chain**: Each block links to previous via hash
3. **Balance Management**: Automatically recalculated from entire chain
4. **Transaction Validation**: Checks balances before adding to mempool
5. **Coinbase Transaction**: First transaction in every mined block, pays reward + fees
6. **Mempool Management**: Pending transactions cleared when mined
7. **Confirmation Formula**: Consistent calculation across all views

## Documentation

- **HALVING_EXPLANATION.md**: Detailed explanation of the halving mechanism
- **PROJECT_SUMMARY.md**: This file - complete project overview

## Build Status

- Build: Successful
- Warnings: 0
- Errors: 0
- Framework: .NET 8.0
- Project Type: ASP.NET Core MVC

All three tasks have been successfully implemented with a clean, intuitive UI and comprehensive functionality.
