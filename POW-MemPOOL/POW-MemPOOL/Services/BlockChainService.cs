using System;
using System.Threading;
using System.Threading.Tasks;
using POW_MemPOOL.Models;

namespace POW_MemPOOL.Services
{
    public enum MiningStatus { NotStarted, InProgress, Completed, Canceled }

    public class MiningState
    {
        public MiningStatus Status { get; set; } = MiningStatus.NotStarted;
        public int Attempts { get; set; }
        public long ElapsedMs { get; set; }
    }

    public class BlockChainService
    {
        private readonly Blockchain _blockchain = new Blockchain();
        public int Difficulty { get; set; } = 4;

        // State for Async Mining
        private static CancellationTokenSource _cancellationTokenSource;
        private static Task<Block> _miningTask;
        private static readonly MiningState _miningState = new MiningState();
        private static readonly object _lock = new object();

        public Blockchain GetBlockchain() => _blockchain;

        public void AddBlock(string data)
        {
            var latestBlock = _blockchain.GetLatestBlock();
            var newBlock = new Block(latestBlock.Id + 1, data, DateTime.UtcNow, latestBlock.Hash);
            newBlock.Mine(Difficulty);
            _blockchain.AddBlock(newBlock);
        }

        public void StartMiningAsync(string data)
        {
            lock (_lock)
            {
                if (_miningState.Status == MiningStatus.InProgress)
                {
                    return; // Already mining
                }

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;
                var progress = new Progress<int>(attempts => {
                    _miningState.Attempts = attempts;
                });

                _miningState.Status = MiningStatus.InProgress;
                _miningState.Attempts = 0;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var latestBlock = _blockchain.GetLatestBlock();
                var newBlock = new Block(latestBlock.Id + 1, data, DateTime.UtcNow, latestBlock.Hash);

                _miningTask = newBlock.MineAsync(Difficulty, token, progress);

                _miningTask.ContinueWith(task =>
                {
                    stopwatch.Stop();
                    lock (_lock)
                    {
                        if (task.IsCanceled)
                        {
                            _miningState.Status = MiningStatus.Canceled;
                        }
                        else if (task.IsFaulted)
                        {
                            // Handle error, maybe log it
                            _miningState.Status = MiningStatus.Canceled; // Or an error state
                        }
                        else
                        {
                            var minedBlock = task.Result;
                            _blockchain.AddBlock(minedBlock);
                            _miningState.Status = MiningStatus.Completed;
                        }
                        _miningState.ElapsedMs = stopwatch.ElapsedMilliseconds;
                    }
                }, TaskScheduler.Default);
            }
        }

        public void CancelMining()
        {
            lock (_lock)
            {
                if (_miningState.Status == MiningStatus.InProgress)
                {
                    _cancellationTokenSource?.Cancel();
                }
            }
        }

        public MiningState GetMiningStatus()
        {
            lock (_lock)
            {
                // To avoid returning a reference that can be modified
                return new MiningState
                {
                    Status = _miningState.Status,
                    Attempts = _miningState.Attempts,
                    ElapsedMs = _miningState.ElapsedMs
                };
            }
        }
        
        public bool IsChainValid()
        {
            return _blockchain.IsValid();
        }
    }
}
