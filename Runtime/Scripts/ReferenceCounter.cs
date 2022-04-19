namespace UnityGltf
{
	public class ReferenceCounter<T>
	{
		public T reference;
		public int counter;

		public ReferenceCounter(T reference)
		{
			this.reference = reference;
		}
	}
}