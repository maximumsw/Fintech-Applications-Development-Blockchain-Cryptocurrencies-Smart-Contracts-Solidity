# Halving Mechanism Explanation

## Overview

This blockchain implements a **halving mechanism** similar to Bitcoin, where the mining reward decreases over time at regular intervals. This document explains how the halving logic works in this implementation.

## Halving Parameters

The halving mechanism is controlled by two key parameters in `BlockchainService`:

- **`BaseReward`**: The initial block reward (default: 50 coins)
- **`HalvingInterval`**: Number of blocks between each halving event (default: 10 blocks)

## Halving Formula

The block reward at any given block height is calculated using the following formula:

```
reward = BaseReward / 2^k
```

Where:
- `k` = number of halvings that have occurred
- `k` = `blockHeight / HalvingInterval` (integer division)

## Implementation

The halving logic is implemented in the `GetBlockReward(int blockHeight)` method in `BlockchainService.cs`:

```csharp
public decimal GetBlockReward(int blockHeight)
{
    if (blockHeight == 0)
        return 0; // Genesis block has no mining reward

    int halvings = blockHeight / HalvingInterval;
    decimal reward = BaseReward;

    for (int i = 0; i < halvings; i++)
    {
        reward /= 2m;
    }

    return reward;
}
```

## Reward Schedule Example

With the default settings (`BaseReward = 50`, `HalvingInterval = 10`):

| Block Range | Halvings (k) | Reward Calculation | Reward (coins) |
|-------------|--------------|-------------------|----------------|
| 0           | 0            | Genesis           | 0              |
| 1-10        | 0            | 50 / 2^0          | 50.00          |
| 11-20       | 1            | 50 / 2^1          | 25.00          |
| 21-30       | 2            | 50 / 2^2          | 12.50          |
| 31-40       | 3            | 50 / 2^3          | 6.25           |
| 41-50       | 4            | 50 / 2^4          | 3.125          |
| 51-60       | 5            | 50 / 2^5          | 1.5625         |
| ... and so on | ...       | ...               | ...            |

## Total Miner Compensation

When a block is mined, the miner receives:

```
Total Compensation = Block Reward + Total Transaction Fees
```

This is implemented in the coinbase transaction:

```csharp
var blockReward = GetBlockReward(newBlockHeight);
var totalFees = Mempool.Sum(tx => tx.Fee);
var coinbaseAmount = blockReward + totalFees;

var coinbaseTx = new Transaction("COINBASE", minerAddress, coinbaseAmount, 0,
    $"Block reward (base: {blockReward}, fees: {totalFees})");
```

## Key Features

1. **Deflationary Supply**: The halving mechanism ensures that new coin creation decreases over time, making the cryptocurrency deflationary.

2. **Fee Incentive**: As block rewards decrease through halvings, transaction fees become increasingly important for miner incentives.

3. **Predictable Schedule**: The halving occurs at fixed block intervals, making the reward schedule completely predictable and transparent.

4. **Genesis Block Exception**: Block 0 (genesis block) has no mining reward since it's the initial block that bootstraps the network.

## Configuration

You can customize the halving behavior by modifying the parameters in `BlockchainService`:

```csharp
public decimal BaseReward { get; set; } = 50m;  // Initial reward
public int HalvingInterval { get; set; } = 10;   // Blocks between halvings
```

For example:
- Setting `HalvingInterval = 5` would cause halvings to occur more frequently
- Setting `BaseReward = 100m` would double the initial block reward
- Bitcoin uses `BaseReward = 50 BTC` and `HalvingInterval = 210,000 blocks`

## Viewing Halving Effects in the UI

You can observe the halving mechanism in action through the web interface:

1. **Mine Block Page**: Shows the "Next Block Reward" which decreases after each halving
2. **Block Details**: The coinbase transaction note shows the breakdown: `Block reward (base: X, fees: Y)`
3. **Wallet History**: Miners can see their rewards decreasing over time as halvings occur
