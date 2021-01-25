using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public GameObject KeybindPrefab;
    public GameObject InputPrefab;

    public override void InstallBindings()
    {
        Container.Bind<KeybindManager>().FromComponentInNewPrefab(KeybindPrefab).AsSingle().NonLazy();
        Container.Bind<InputSystem>().FromComponentInNewPrefab(InputPrefab).AsSingle().NonLazy();
    }
}