using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public enum UILifeCycleState
    {
        None,
        Opening,
        Opened,
        Paused,
        Resumed,
        Closing,
        Closed
    }

    public abstract class BasePanel : MonoBehaviour
    {
        public abstract string UIName { get; }

        protected UILifeCycleState _state = UILifeCycleState.None;
        public UILifeCycleState State => _state;

        public abstract void InitUI();

        protected virtual void Awake()
        {
            _state = UILifeCycleState.None;
            InitUI();
        }

        protected virtual void OnEnable()
        {
            if (_state == UILifeCycleState.Paused)
            {
                _state = UILifeCycleState.Resumed;
                OnResume();
            }
        }

        protected virtual void OnDisable()
        {
            if (_state == UILifeCycleState.Opened)
            {
                _state = UILifeCycleState.Paused;
                OnPause();
            }
        }

        public virtual void OpenPanel()
        {
            _state = UILifeCycleState.Opening;
            SetActive(true);
            OnBeforeOpen();
            _state = UILifeCycleState.Opened;
            OnOpened();
        }

        public virtual void ClosePanel()
        {
            if (_state == UILifeCycleState.Closing || _state == UILifeCycleState.Closed)
                return;

            _state = UILifeCycleState.Closing;
            OnBeforeClose();
            SetActive(false);
            _state = UILifeCycleState.Closed;

            if (_state == UILifeCycleState.Closed)
            {
                Destroy(gameObject);
            }
        }

        public virtual void SetActive(bool _bool)
        {
            gameObject.SetActive(_bool);
        }

        protected virtual void OnBeforeOpen() { }
        protected virtual void OnOpened() { }
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnBeforeClose() { }
    }
}
