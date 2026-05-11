# SQA Engineer Agent Memory

## k6 Docker Execution — Key Facts

- **Docker MCP cannot spawn k6 containers.** The MCP Gateway only manages MCP server lifecycle. Use the `Bash` tool to run `docker run grafana/k6 ...` directly.
- **No `Testcontainers.K6` for .NET.** Use generic `ContainerBuilder` from Testcontainers.NET. Java and Go have official K6 modules; .NET does not (confirmed NuGet, May 2026).
- **`grafana/k6-action` is archived (deprecated July 2024).** Always use `grafana/setup-k6-action@v1` + `grafana/run-k6-action@v1`.
- **Exit code 99 = threshold failure** (primary CI gate). Exit 107 = script JS exception. Exit 105 = user cancel (not a test failure).
- **`--discard-response-bodies` should be set by default** in all CI load/stress tests — reduces container memory and GC pressure for more accurate latency measurements.
- **Host URL from container:** use `host.docker.internal` on Docker Desktop (Windows/macOS); add `--add-host=host.docker.internal:host-gateway` on Linux Docker Engine.
- **InfluxDB v1 output** (`--out influxdb=...`) is built into `grafana/k6`. InfluxDB v2 requires a custom xk6 build from `grafana/xk6-output-influxdb`.
- **`if: always()` on artifact upload** is mandatory — without it, CI skips the upload when k6 exits with 99 (threshold failure), losing all evidence.

## k6 Skill Map

| Goal | Skill |
|------|-------|
| Write/configure a performance test script | `k6-performance-testing` |
| Write/configure a stress/spike/breakpoint test | `k6-stress-testing` |
| Write/configure a load/soak/multi-scenario test | `k6-load-testing` |
| Run any k6 test via Docker (dev, CI, observability) | `k6-docker` |
