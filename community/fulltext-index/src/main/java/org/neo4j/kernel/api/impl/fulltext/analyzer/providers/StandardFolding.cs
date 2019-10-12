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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext.analyzer.providers
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using AnalyzerProvider = Org.Neo4j.Graphdb.index.fulltext.AnalyzerProvider;
	using Service = Org.Neo4j.Helpers.Service;
	using StandardFoldingAnalyzer = Org.Neo4j.Index.impl.lucene.@explicit.StandardFoldingAnalyzer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(AnalyzerProvider.class) public class StandardFolding extends org.neo4j.graphdb.index.fulltext.AnalyzerProvider
	public class StandardFolding : AnalyzerProvider
	{
		 public const string STANDARD_FOLDING_ANALYZER_NAME = "standard-folding";

		 public StandardFolding() : base(STANDARD_FOLDING_ANALYZER_NAME)
		 {
		 }

		 public override Analyzer CreateAnalyzer()
		 {
			  return new StandardFoldingAnalyzer();
		 }

		 public override string Description()
		 {
			  return "Analyzer that uses ASCIIFoldingFilter to remove accents (diacritics). Otherwise behaves as standard " +
						 "english analyzer.";
		 }
	}

}