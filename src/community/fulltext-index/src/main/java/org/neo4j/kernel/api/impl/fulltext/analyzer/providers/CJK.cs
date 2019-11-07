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
	using CJKAnalyzer = org.apache.lucene.analysis.cjk.CJKAnalyzer;

	using AnalyzerProvider = Neo4Net.GraphDb.Index.fulltext.AnalyzerProvider;
	using Service = Neo4Net.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(AnalyzerProvider.class) public class CJK extends Neo4Net.GraphDb.Index.fulltext.AnalyzerProvider
	public class CJK : AnalyzerProvider
	{
		 public CJK() : base("cjk")
		 {
		 }

		 public override Analyzer CreateAnalyzer()
		 {
			  return new CJKAnalyzer();
		 }

		 public override string Description()
		 {
			  return "CJK - Chinese/Japanese/Korean - analyzer. Terms are normalised and case-folded. Produces bi-grams, and filters out stop words.";
		 }
	}

}