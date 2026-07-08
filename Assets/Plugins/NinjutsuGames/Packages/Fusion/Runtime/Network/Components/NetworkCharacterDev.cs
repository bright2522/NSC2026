using GameCreator.Runtime.Characters;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class NetworkCharacterDev : Character
    {
        public bool updateOnRender = true;
        protected override void Update()
        {
            if(!updateOnRender) base.Update();
        }

        public void UpdateRender()
        {
            if(updateOnRender) base.Update();
        }
    }
}