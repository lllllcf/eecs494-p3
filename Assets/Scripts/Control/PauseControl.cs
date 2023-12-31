using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseControl : MonoBehaviour
{
    private Sprite mouseGameImage;
    private Sprite keyboardGameImage;
    private bool isPaused;
    private bool isDialogBlocking;

    private void Awake()
    {
        EventBus.Subscribe<DialogBlockingEvent>(e => isDialogBlocking = e.status);
        EventBus.Subscribe<TriggerPauseEvent>(_OnTriggerPause);
        mouseGameImage = Resources.Load<Sprite>("Sprites/Background/MouseGame");
        keyboardGameImage = Resources.Load<Sprite>("Sprites/Background/KeyboardGame");
    }

    private void Start()
    {
        isPaused = false;
        isDialogBlocking = false;
    }

    private void _OnTriggerPause(TriggerPauseEvent _)
    {
        if (!isPaused && GameProgressControl.isGameActive && !isDialogBlocking)
        {
            SetPauseState(true);
            EventBus.Publish(new UpdateCursorEvent(null));
            if (SceneManager.GetActiveScene().name == StringPool.mainTutorialScene)
            {
                EventBus.Publish(new DisplayDialogEvent(
                    "Tutorial paused!", "Take a rest.",
                    new Dictionary<string, Tuple<UnityAction, bool>>()
                    {
                        { "Resume", new Tuple<UnityAction, bool>(() => SetPauseState(false), true) },
                        {
                            "Restart", new Tuple<UnityAction, bool>(
                                () =>
                                {
                                    SceneState.SetTransition(
                                        1, 0, StringPool.mainTutorialScene, mouseGameImage, keyboardGameImage
                                    );
                                    EventBus.Publish(new TransitSceneEvent());
                                }, true
                            )
                        },
                        {
                            "Skip", new Tuple<UnityAction, bool>(
                                () =>
                                {
                                    SceneState.SetTransition(
                                        1, 0, StringPool.mainGameScene, mouseGameImage, keyboardGameImage
                                    );
                                    EventBus.Publish(new TransitSceneEvent());
                                }, true
                            )
                        }
                    }
                ));
            }
            else
            {
                EventBus.Publish(new DisplayDialogEvent(
                    "Game paused!", "Take a rest.",
                    new Dictionary<string, Tuple<UnityAction, bool>>()
                    {
                        { "Resume", new Tuple<UnityAction, bool>(() => SetPauseState(false), true) },
                        {
                            "Restart", new Tuple<UnityAction, bool>(
                                () =>
                                {
                                    SceneState.SetTransition(
                                        1, 0, StringPool.mainGameScene, mouseGameImage, keyboardGameImage
                                    );
                                    EventBus.Publish(new TransitSceneEvent());
                                }, true
                            )
                        },
                        {
                            "Exit", new Tuple<UnityAction, bool>(
                                () =>
                                {
                                    SceneState.SetTransition(
                                        1, 2, StringPool.mainMenuScene, mouseGameImage, keyboardGameImage
                                    );
                                    EventBus.Publish(new TransitSceneEvent());
                                }, true
                            )
                        }
                    }
                ));
            }
        }
    }

    private void SetPauseState(bool status)
    {
        isPaused = status;
        EventBus.Publish(new ModifyPauseEvent(isPaused));
    }
}