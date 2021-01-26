namespace CNCMaps.Shared {

	// todo: Examine all clases where this is used. Take care of the new RandomMapGenerationFailed
	public enum EngineResult {
		RenderedOk,
		Exception,
		LoadTheaterFailed,
		LoadRulesFailed,
		RandomMapGenerationFailed
	}

}
