using System.Collections;
using TaigaGames.Saves;
using UnityEngine;

namespace TaigaGames
{
    public abstract class GameBehaviour<T> : MonoBehaviour where T : class, IGameData, new()
    {
        [SerializeField] protected SaveManager<T> _saveManager;

        protected T Data => _saveManager.Data;
        
        public bool Initialized { get; private set; }
        
        protected virtual void Reset()
        {
            _saveManager = FindObjectOfType<SaveManager<T>>();
        }

        protected IEnumerator Start()
        {
            yield return new WaitUntil(() => _saveManager.Data != null);
            yield return Initialize();
            Initialized = true;
        }

        protected virtual IEnumerator Initialize()
        {
            yield return true;
        }
    }
}