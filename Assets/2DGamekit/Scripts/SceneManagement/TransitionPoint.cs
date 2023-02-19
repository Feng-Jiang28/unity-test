using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Models;
using Proyecto26;
using UnityEngine.Networking;
using System.Collections.Generic;


namespace Gamekit2D
{
    [RequireComponent(typeof(Collider2D))]
    public class TransitionPoint : MonoBehaviour
    {
        public enum TransitionType
        {
            DifferentZone, DifferentNonGameplayScene, SameScene,
        }


        public enum TransitionWhen
        {
            ExternalCall, InteractPressed, OnTriggerEnter,
        }

    
        [Tooltip("This is the gameobject that will transition.  For example, the player.")]
        public GameObject transitioningGameObject;
        [Tooltip("Whether the transition will be within this scene, to a different zone or a non-gameplay scene.")]
        public TransitionType transitionType;
        [SceneName]
        public string newSceneName;
        [Tooltip("The tag of the SceneTransitionDestination script in the scene being transitioned to.")]
        public SceneTransitionDestination.DestinationTag transitionDestinationTag;
        [Tooltip("The destination in this scene that the transitioning gameobject will be teleported.")]
        public TransitionPoint destinationTransform;
        [Tooltip("What should trigger the transition to start.")]
        public TransitionWhen transitionWhen;
        [Tooltip("The player will lose control when the transition happens but should the axis and button values reset to the default when control is lost.")]
        public bool resetInputValuesOnTransition = true;
        [Tooltip("Is this transition only possible with specific items in the inventory?")]
        public bool requiresInventoryCheck;
        [Tooltip("The inventory to be checked.")]
        public InventoryController inventoryController;
        [Tooltip("The required items.")]
        public InventoryController.InventoryChecker inventoryCheck;
    
        bool m_TransitioningGameObjectPresent;

        private readonly string basePath = "https://datacollection-54f60-default-rtdb.firebaseio.com/";
	    private RequestHelper currentRequest;


        void Start ()
        {
            if (transitionWhen == TransitionWhen.ExternalCall)
                m_TransitioningGameObjectPresent = true;
        }

        void OnTriggerEnter2D (Collider2D other)
        {
            if (other.gameObject == transitioningGameObject)
            {
                m_TransitioningGameObjectPresent = true;

                if (ScreenFader.IsFading || SceneController.Transitioning)
                    return;

                if (transitionWhen == TransitionWhen.OnTriggerEnter)
                    TransitionInternal ();
            }
        }

        void OnTriggerExit2D (Collider2D other)
        {
            if (other.gameObject == transitioningGameObject)
            {
                m_TransitioningGameObjectPresent = false;
            }
        }

        void Update ()
        {
            if (ScreenFader.IsFading || SceneController.Transitioning)
                return;

            if(!m_TransitioningGameObjectPresent)
                return;

            if (transitionWhen == TransitionWhen.InteractPressed)
            {
                if (PlayerInput.Instance.Interact.Down)
                {
                    TransitionInternal ();
                }
            }
        }

        protected void TransitionInternal ()
        {
            
            if (requiresInventoryCheck)
            {
                if(!inventoryCheck.CheckInventory (inventoryController)){
                    
                    if(newSceneName == "Zone5"){
                        Debug.Log("here at Zone 5");  
                        RestClient.DefaultRequestParams["param1"] = "My first param";
                        RestClient.DefaultRequestParams["param3"] = "My other param";

                        currentRequest = new RequestHelper {
                            Uri = basePath + "/posts.json",
                            Params = new Dictionary<string, string> {
                                { "param1", "value 1" },
                                { "param2", "value 2" }
                            },
                            Body = new Post {
                                title = "foo",
                                body = "bar",
                                userId = 1
                            },
                            EnableDebug = true
                        };
                        RestClient.Post<Post>(currentRequest)
                        .Then(res => {

                            // And later we can clear the default query string params for all requests
                            RestClient.ClearDefaultParams();

                            //this.LogMessage("Success", JsonUtility.ToJson(res, true));
                        })
                        .Catch(err => Debug.Log("Failed."));
                    }
                    return;
                }
            }
            if (transitionType == TransitionType.SameScene)
            {
                
                GameObjectTeleporter.Teleport (transitioningGameObject, destinationTransform.transform);

            }
            else
            {
               
                SceneController.TransitionToScene (this);
            }
        }

        public void Transition ()
        {
            if(!m_TransitioningGameObjectPresent)
                return;

            if(transitionWhen == TransitionWhen.ExternalCall)
                TransitionInternal ();
        }
    }
}