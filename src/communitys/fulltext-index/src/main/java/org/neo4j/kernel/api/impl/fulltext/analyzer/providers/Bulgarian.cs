/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Api.Impl.Fulltext.analyzer.providers
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using BulgarianAnalyzer = org.apache.lucene.analysis.bg.BulgarianAnalyzer;

	using AnalyzerProvider = Neo4Net.Graphdb.index.fulltext.AnalyzerProvider;
	using Service = Neo4Net.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(AnalyzerProvider.class) public class Bulgarian extends org.neo4j.graphdb.index.fulltext.AnalyzerProvider
	public class Bulgarian : AnalyzerProvider
	{
		 public Bulgarian() : base("bulgarian")
		 {
		 }

		 public override Analyzer CreateAnalyzer()
		 {
			  return new BulgarianAnalyzer();
		 }

		 public override string Description()
		 {
			  return "Bulgarian analyzer with light stemming, as specified by \"Searching Strategies for the Bulgarian Language\", and stop word filtering.";
		 }
	}

}