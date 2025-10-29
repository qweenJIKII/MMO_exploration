// ゲーム状態管理: Title/InGame/Loading/Pause状態の制御
using System;
using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// ゲーム全体の状態を管理するシングルトン
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public enum GameState
        {
            Initializing,   // 初期化中
            Title,          // タイトル画面
            Loading,        // ロード中
            InGame,         // ゲームプレイ中
            Paused,         // 一時停止
            Transition      // 遷移中
        }

        private static GameStateManager _instance;
        public static GameStateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameStateManager");
                    _instance = go.AddComponent<GameStateManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private GameState _currentState = GameState.Initializing;
        private GameState _previousState = GameState.Initializing;

        // イベント
        public event Action<GameState, GameState> OnStateChanged;

        public GameState CurrentState => _currentState;
        public GameState PreviousState => _previousState;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 初期化完了後、タイトル画面へ遷移
            ChangeState(GameState.Title);
        }

        /// <summary>
        /// 状態を変更
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (_currentState == newState)
            {
                Debug.LogWarning($"[GameStateManager] Already in state: {newState}");
                return;
            }

            _previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameStateManager] State changed: {_previousState} -> {_currentState}");

            // 状態変更時の処理
            OnStateExit(_previousState);
            OnStateEnter(_currentState);

            // イベント発火
            OnStateChanged?.Invoke(_previousState, _currentState);
        }

        /// <summary>
        /// 状態に入る時の処理
        /// </summary>
        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.Initializing:
                    // 初期化処理
                    break;
                case GameState.Title:
                    Time.timeScale = 1f;
                    break;
                case GameState.Loading:
                    // ロード画面表示
                    break;
                case GameState.InGame:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Transition:
                    // 遷移中
                    break;
            }
        }

        /// <summary>
        /// 状態から出る時の処理
        /// </summary>
        private void OnStateExit(GameState state)
        {
            switch (state)
            {
                case GameState.Paused:
                    Time.timeScale = 1f;
                    break;
            }
        }

        /// <summary>
        /// ゲームを一時停止
        /// </summary>
        public void Pause()
        {
            if (_currentState == GameState.InGame)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// ゲームを再開
        /// </summary>
        public void Resume()
        {
            if (_currentState == GameState.Paused)
            {
                ChangeState(GameState.InGame);
            }
        }

        /// <summary>
        /// ゲームプレイを開始
        /// </summary>
        public void StartGame()
        {
            ChangeState(GameState.InGame);
        }

        /// <summary>
        /// タイトル画面に戻る
        /// </summary>
        public void ReturnToTitle()
        {
            ChangeState(GameState.Title);
        }

        /// <summary>
        /// ロード状態に遷移
        /// </summary>
        public void StartLoading()
        {
            ChangeState(GameState.Loading);
        }

        /// <summary>
        /// 現在の状態が指定した状態かチェック
        /// </summary>
        public bool IsInState(GameState state)
        {
            return _currentState == state;
        }

        /// <summary>
        /// 現在の状態がいずれかの状態に該当するかチェック
        /// </summary>
        public bool IsInAnyState(params GameState[] states)
        {
            foreach (var state in states)
            {
                if (_currentState == state)
                    return true;
            }
            return false;
        }
    }
}
