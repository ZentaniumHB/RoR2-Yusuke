using System;
using System.Collections.Generic;
using RoR2;


namespace YusukeMod.Characters.Survivors.Yusuke.Components
{
    public class YusukeCSS : CharacterSelectSurvivorPreviewDisplayController
    {

        // Used for the Yusuke model displayed on the character select screen

        private new void OnEnable()
        {

            currentLoadout = Loadout.RequestInstance();
            NetworkUser.onLoadoutChangedGlobal += OnLoadoutChangedGlobal;
            RoR2Application.onNextUpdate += Refresh;
            RunDefaultResponses();

            Util.PlaySound("Play_MusicYuYuHakushoLobbyOpening", gameObject);
        }

        private new void OnDisable()
        {
            NetworkUser.onLoadoutChangedGlobal -= OnLoadoutChangedGlobal;
            currentLoadout = Loadout.ReturnInstance(currentLoadout);
        }

        private new void Refresh()
        {
            if (this && networkUser)
                OnLoadoutChangedGlobal(networkUser);
        }



    }
}
