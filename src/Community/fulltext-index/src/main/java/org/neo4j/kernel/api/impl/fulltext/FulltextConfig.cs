/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Standard = Neo4Net.Kernel.Api.Impl.Fulltext.analyzer.providers.Standard;

	using Description = Neo4Net.Configuration.Description;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.Graphdb.config;
	using Settings = Neo4Net.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	/// <summary>
	/// Configuration settings for the fulltext index.
	/// </summary>
	public class FulltextConfig : LoadableConfig
	{
		 private const string _defaultAnalyzer = Standard.STANDARD_ANALYZER_NAME;

		 [Description("The name of the analyzer that the fulltext indexes should use by default.")]
		 public static readonly Setting<string> FulltextDefaultAnalyzer = setting( "dbms.index.fulltext.default_analyzer", STRING, _defaultAnalyzer );

		 [Description("Whether or not fulltext indexes should be eventually consistent by default or not.")]
		 public static readonly Setting<bool> EventuallyConsistent = setting( "dbms.index.fulltext.eventually_consistent", BOOLEAN, Settings.FALSE );

		 [Description("The eventually_consistent mode of the fulltext indexes works by queueing up index updates to be applied later in a background thread. " + "This setting sets an upper bound on how many index updates are allowed to be in this queue at any one point in time. When it is reached, " + "the commit process will slow down and wait for the index update applier thread to make some more room in the queue.")]
		 public static readonly Setting<int> EventuallyConsistentIndexUpdateQueueMaxLength = buildSetting( "dbms.index.fulltext.eventually_consistent_index_update_queue_max_length", INTEGER, "10000" ).constraint( min( 1 ) ).constraint( max( 50_000_000 ) ).build();
	}

}