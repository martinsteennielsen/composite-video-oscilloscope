
namespace CompositeVideoOscilloscope {
    public interface IContent<T> {
        T Get();
        void Next();
        void Start(int currentLine);
    }
}
