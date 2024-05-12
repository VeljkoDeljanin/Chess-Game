using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {

    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnOpponentDisconnect;
    public event EventHandler OnGameStartSound;
    public event EventHandler OnTenSecondsSound;
    
    private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        Promotion,
        GameOver
    }

    [SerializeField] private GameObject[] teamCameras;

    public bool isWhiteTurn;

    private NetworkVariable<FixedString64Bytes> resultText = new NetworkVariable<FixedString64Bytes>();

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> whitePlayerGamePlayingTimer = new NetworkVariable<float>();
    private NetworkVariable<float> blackPlayerGamePlayingTimer = new NetworkVariable<float>();
    private Dictionary<ulong, bool> playerReadyDictionary;
    private int promotionColorId;
    private bool whitePlayerTenSecondsSoundPlayed = false;
    private bool blackPlayerTenSecondsSoundPlayed = false;

    private void Awake() {
        Instance = this;

        isWhiteTurn = true;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start() {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        GameInput.Instance.OnKeyPressed += GameInput_OnKeyPressed;

        ChangeCamera(GameMultiplayer.Instance.GetPlayerData().colorId);

        whitePlayerGamePlayingTimer.Value = GameMultiplayer.Instance.GetGamePlayingTimeNumber();
        blackPlayerGamePlayingTimer.Value = GameMultiplayer.Instance.GetGamePlayingTimeNumber();
    }

    private void Update() {
        ProcessTenSecondsEvent();

        if (!IsServer) {
            return;
        }

        switch (state.Value) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f) {
                    state.Value = State.GamePlaying;
                }
                break;
            case State.GamePlaying:
                if (GameMultiplayer.Instance.GetGamePlayingTimerId() != 0) {
                    if (isWhiteTurn) {
                        whitePlayerGamePlayingTimer.Value -= Time.deltaTime;
                        if (whitePlayerGamePlayingTimer.Value < 0f) {
                            ProcessGameOver(TeamColor.Black);
                        }
                    } else {
                        blackPlayerGamePlayingTimer.Value -= Time.deltaTime;
                        if (blackPlayerGamePlayingTimer.Value < 0f) {
                            ProcessGameOver(TeamColor.White);
                        }
                    }
                }
                break;
            case State.Promotion:
                break;
            case State.GameOver:
                break;
        }
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue) {
        OnStateChanged?.Invoke(this, EventArgs.Empty);

        if (previousValue == State.CountdownToStart) {
            OnGameStartSound?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ProcessTenSecondsEvent() {
        if (GameMultiplayer.playMultiplayer && state.Value == State.GamePlaying) {
            if (!whitePlayerTenSecondsSoundPlayed && isWhiteTurn && GameMultiplayer.Instance.GetPlayerData().colorId == 0 && Mathf.CeilToInt(whitePlayerGamePlayingTimer.Value) == 10) {
                whitePlayerTenSecondsSoundPlayed = true;

                OnTenSecondsSound?.Invoke(this, EventArgs.Empty);
            } else if (!blackPlayerTenSecondsSoundPlayed && !isWhiteTurn && GameMultiplayer.Instance.GetPlayerData().colorId == 1 && Mathf.CeilToInt(blackPlayerGamePlayingTimer.Value) == 10) {
                blackPlayerTenSecondsSoundPlayed = true;

                OnTenSecondsSound?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        OnOpponentDisconnect?.Invoke(this, EventArgs.Empty);
    }

    public void SetTeamPromotion(int promotionColorId) {
        SetTeamPromotionServerRpc(promotionColorId);
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetTeamPromotionServerRpc(int promotionColorId) {
        SetTeamPromotionClientRpc(promotionColorId);

        state.Value = State.Promotion;
    }

    [ClientRpc]
    private void SetTeamPromotionClientRpc(int promotionColorId) {
        this.promotionColorId = promotionColorId;
    }

    public void TeamPromotionOver() {
        state.Value = State.GamePlaying;
    }

    public void SetGameDraw() {
        resultText.Value = "Draw!";
        state.Value = State.GameOver;
    }

    public void ProcessGameOver(TeamColor winningTeam) {
        ProcessGameOverServerRpc(winningTeam);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ProcessGameOverServerRpc(TeamColor winningTeam) {
        if (winningTeam == TeamColor.None) {
            resultText.Value = "Draw!";
        } else if (GameMultiplayer.playMultiplayer) {
            resultText.Value = GameMultiplayer.Instance.GetPlayerName((int)winningTeam - 1) + " wins!";
        } else {
            if (winningTeam == TeamColor.White) {
                resultText.Value = "White team wins!";
            } else {
                resultText.Value = "Black team wins!";
            }
        }

        state.Value = State.GameOver;
    }

    private void GameInput_OnKeyPressed(object sender, EventArgs e) {
        isLocalPlayerReady = true;
        OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default) {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady) {
            state.Value = State.CountdownToStart;
        }
    }

    private void ChangeCamera(int cameraIndex) {
        foreach (GameObject camera in teamCameras)
            camera.SetActive(false);

        teamCameras[cameraIndex].SetActive(true);
    }

    public int CheckForCheckmate(TeamColor team) {
        // Getting the king we are checking
        Piece ourKing = null;
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                if (PieceManager.Instance.pieces[i, j] != null)
                    if (PieceManager.Instance.pieces[i, j].type == PieceType.King && PieceManager.Instance.pieces[i, j].team == team)
                        ourKing = PieceManager.Instance.pieces[i, j];

        // Is king in check?
        bool kingChecked = false;
        for (int i = 0; i < TileManager.TILE_COUNT; i++) {
            for (int j = 0; j < TileManager.TILE_COUNT; j++) {
                if (PieceManager.Instance.pieces[i, j] != null && PieceManager.Instance.pieces[i, j].team != ourKing.team) {
                    List<Vector2Int> enemyMoves = PieceManager.Instance.pieces[i, j].GetValidMoves(ref PieceManager.Instance.pieces, TileManager.TILE_COUNT, PieceManager.Instance.lastMove);

                    if (GameInput.Instance.ContainsMove(ref enemyMoves, new Vector2Int(ourKing.currentX, ourKing.currentY))) {
                        kingChecked = true;
                        break;
                    }
                }
            }
        }

        // Do we have any moves left?
        int movesLeft = 0;
        for (int i = 0; i < TileManager.TILE_COUNT; i++) {
            for (int j = 0; j < TileManager.TILE_COUNT; j++) {
                if (PieceManager.Instance.pieces[i, j] != null && PieceManager.Instance.pieces[i, j].team == ourKing.team) {
                    PieceManager.Instance.currentPiece = PieceManager.Instance.pieces[i, j];
                    PieceManager.Instance.validMoves = PieceManager.Instance.pieces[i, j].GetValidMoves(ref PieceManager.Instance.pieces, TileManager.TILE_COUNT, PieceManager.Instance.lastMove);
                    PieceManager.Instance.PreventMove();
                    if (PieceManager.Instance.validMoves.Count > 0)
                        movesLeft++;
                }
            }
        }

        PieceManager.Instance.validMoves.Clear();
        PieceManager.Instance.currentPiece = null;

        if (movesLeft == 0) {
            if (kingChecked) {
                // Checkmate
                ProcessGameOver(team == TeamColor.White ? TeamColor.Black : TeamColor.White);
            } else {
                // Stalemate
                ProcessGameOver(TeamColor.None);
            }
            return 1;
        }

        if (kingChecked) {
            return 2;
        }

        return 0;
    }

    public bool IsWaitingToStartActive() {
        return state.Value == State.WaitingToStart;
    }

    public bool IsCountdownToStartActive() {
        return state.Value == State.CountdownToStart;
    }

    public bool IsGamePlayingActive() {
        return state.Value == State.GamePlaying;
    }

    public bool IsPromotionActive() {
        return state.Value == State.Promotion;
    }

    public bool IsGameOverActive() {
        return state.Value == State.GameOver;
    }

    public bool IsLocalPlayerReady() {
        return isLocalPlayerReady;
    }

    public float GetCountdownToStartTimer() {
        return countdownToStartTimer.Value;
    }

    public string GetResultText() {
        return resultText.Value.ToString();
    }

    public float GetWhitePlayerGamePlayingTimer() {
        return whitePlayerGamePlayingTimer.Value;
    }

    public float GetBlackPlayerGamePlayingTimer() {
        return blackPlayerGamePlayingTimer.Value;
    }

    public int GetPromotionColorId() {
        return promotionColorId;
    }

    public override void OnDestroy() {
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }
}
