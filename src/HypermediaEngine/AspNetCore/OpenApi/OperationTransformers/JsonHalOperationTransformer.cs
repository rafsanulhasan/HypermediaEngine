namespace HypermediaEngine.OpenApi.OperationTransformers;

internal class JsonHalOperationTransformer<T>(bool isList, bool withExample)
        : JsonHalOperationTransformer(typeof(T), isList, withExample)
{
}

internal class JsonHalOperationTransformer(Type type, bool isList, bool withExample)
        : CompositeOperationTransformer(
            [
                new JsonOperationTransformer(type, isList, withExample),
                new HalOperationTransformer(type, isList, withExample),
            ])
{
}
