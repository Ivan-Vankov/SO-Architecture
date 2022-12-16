using ExtEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vaflov {
    public class GlobalGameEventListener : ScriptableObject {
        public GameEventVoid eventRef;
        public ExtEvent response;

        public void CallResponse() {
            response?.Invoke();
        }

#if UNITY_EDITOR
        //[RuntimeInitializeOnLoadMethod]
        //public static void OnRuntimeMethodLoad() {
        //    Debug.Log("After Scene is loaded and game is running");
        //}

        //private void OnEnable() {

        //    SceneManager.sceneLoaded += OnLevelFinishedLoading;
        //    if (eventRef) {
        //        eventRef.action += CallResponse;
        //    }
        //}

        //void OnDisable() {
        //    //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        //    SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        //    if (eventRef) {
        //        eventRef.action -= CallResponse;
        //    }
        //}

        //void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        //    Debug.Log("Level Loaded");
        //    Debug.Log(scene.name);
        //    Debug.Log(mode);
        //}
#else

#endif
    }
}