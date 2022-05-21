
//�}�b�`���O�����������A�Q�[���̃T�[�o�i�R���AGM�j�Ƃ̃A�N�Z�X�C���^�[�t�F�C�X

public interface IGameServer
{
    delegate void UpdateCallback(UpdateData data,string abort);
//�T�[�o����̒ʐM����M����R�[���o�b�N
    void SetUpdateCallback(UpdateCallback callback);

//�����f�[�^�i���̃Q�[���̃��[���p�����[�^�ƃ}�b�`���O���ɒ�o�������݂��̃f�[�^�j
    InitialData GetInitialData();

//�Q�[���J�n���������𑗐M
//����Ȍ�A�T�[�o����Q�[���i�s��UpdateCallback���Ăяo�����
    void SendReady();

//�Q�[���ł̑I���𑗐M
    void SendSelect(int phase,int index);

//�����Q�[���I���i�~�Q�j�𑗐M
    void SendSurrender();

//���̃C���^�[�t�F�C�X�̔j��
    void Terminalize();
}

public class UpdateData
{
    public int phase;
    public int damage;

    public class PlayerData
    {
        public int[] draw;
        public int select;
        public int deckcount;
    }
    public PlayerData myself;
    public PlayerData rival;
}

public class InitialData
{
    public int battleSelectTimeLimitSecond;
    public int damageSelectTimeLimitSecond;

    public string myname;
    public string rivalname;


}
