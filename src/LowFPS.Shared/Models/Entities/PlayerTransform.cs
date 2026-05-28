using MessagePack;

namespace LowFPS.Shared.Models.Entities
{

    /// <summary>
    /// MagicOnionでPlayerのtransformをまとめるクラス
    /// </summary>
    [MessagePackObject]
    public class PlayerTransform
    {
        [Key(0)] public SimpleTransform body;
        [Key(1)] public SimpleTransform camera;
        [Key(2)] public SimpleTransform gun;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerTransform()
        {
            this.body = new SimpleTransform();
            this.camera = new SimpleTransform();
            this.gun = new SimpleTransform();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerTransform(SimpleTransform body, SimpleTransform camera, SimpleTransform gun)
        {
            this.body = body;
            this.camera = camera;
            this.gun = gun;
        }
    }
}
