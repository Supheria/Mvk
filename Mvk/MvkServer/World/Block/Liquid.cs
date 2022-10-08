namespace MvkServer.World.Block
{
    public struct Liquid
    {
        private bool water;
        private bool lava;
        private bool oil;

        public bool IsWater() => water;
        public bool IsLava() => lava;
        public bool IsOil() => oil;
        public bool IsAll() => water && lava && oil;

        public void Water() => water = true;
        public void Lava() => lava = true;
        public void Oil() => oil = true;
    }
}
