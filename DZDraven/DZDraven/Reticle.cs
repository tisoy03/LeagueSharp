using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DZDraven
{
    internal class Reticle
    {
        private readonly double _creationTime;
        private readonly double _endTime;
        private readonly int _networkId;
        private readonly GameObject obj;
        private readonly Vector3 posi;

        public Reticle(GameObject retObject, double creatT, Vector3 position, double endT, int nId)
        {
            obj = retObject;
            _creationTime = creatT;
            _endTime = endT;
            _networkId = nId;
            posi = position;
        }

        public GameObject GetObj()
        {
            return obj;
        }

        public Vector3 GetPosition()
        {
            return posi;
        }

        public double GetCreationTime()
        {
            return _creationTime;
        }

        public double GetEndTime()
        {
            return _endTime;
        }

        public int GetNetworkId()
        {
            return _networkId;
        }

        public float DistanceToPlayer()
        {
            return ObjectManager.Player.Distance(GetPosition());
        }
    }
}