using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public GameObject InputPrefab;
    public override void InstallBindings()
    {
        Container.Bind<KeybindManager>().FromComponentInNewPrefab(InputPrefab).AsSingle().NonLazy();
    }
}