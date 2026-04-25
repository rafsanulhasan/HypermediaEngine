namespace HypermediaEngine.OpenApi.OperationTransformers;

internal class HalJsonOperationTransformer<T>(bool isList, bool withExample)
        : HalJsonOperationTransformer(typeof(T), isList, withExample)
{
}

internal class HalJsonOperationTransformer(Type type, bool isList, bool withExample)
        : CompositeOperationTransformer(
            [
                new HalOperationTransformer(type, isList, withExample),
                new JsonOperationTransformer(type, isList, withExample),
            ])
{
}
