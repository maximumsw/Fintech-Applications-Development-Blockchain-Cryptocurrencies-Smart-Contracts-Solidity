# POW-MemPOOL: A Minimal Blockchain MVC Project

This is a .NET MVC application that demonstrates a simple blockchain implementation with Proof-of-Work (PoW) and asynchronous block mining.

## Features

- **Blockchain Simulation**: View a chain of blocks, each cryptographically linked to the previous one.
- **Proof-of-Work (PoW)**: Blocks must be "mined" before being added to the chain. The difficulty of the mining process can be adjusted.
- **Sync & Async Mining**: 
    - **Add Block (Sync)**: The UI will freeze while the block is mined synchronously.
    - **Mine (Async)**: Mines a new block in a background thread without freezing the UI.
- **Asynchronous Mining Controls**:
    - Start mining a new block with any data.
    - View live progress (attempts per second and elapsed time).
    - Cancel a mining operation in progress.
- **Chain Validation**: The application continuously validates the integrity of the blockchain. Any tampering with block data will invalidate the block and the chain.

## How to Run

1. **Prerequisites**:
   - .NET SDK (6.0 or later).
   - A code editor like Visual Studio or VS Code.

2. **Clone & Run**:
   ```sh
   # Clone the repository
   git clone <repository-url>
   cd POW-MemPOOL

   # Run the application
   dotnet run
   ```
3. Open your web browser and navigate to the URL provided in the console (e.g., `https://localhost:7123`).

## How to Use Asynchronous Mining

1. **Start Mining**:
   - On the homepage, locate the **"Mine (Async)"** form.
   - Enter any text into the "Block Data" field.
   - Click the **"Mine (Async)"** button.

2. **Monitor Progress**:
   - The "Add Block" forms will be replaced by a "Mining in Progress..." panel.
   - You will see a live counter of hashing **attempts** and the total **elapsed time**.
   - The UI will remain fully responsive during this process.

3. **Cancel Mining**:
   - To stop the current mining operation, simply click the **"Stop Mining"** button. The operation will be cancelled, and the UI will return to its initial state.

4. **Completion**:
   - Once the mining is successful, a new block will be added to the blockchain.
   - The page will refresh automatically, showing the new block in the table with its calculated `Nonce`, `Difficulty`, and total mining time (`ms`).
   - A notification will appear at the top confirming that mining was completed.