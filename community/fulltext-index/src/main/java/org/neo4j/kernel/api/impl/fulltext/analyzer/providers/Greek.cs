﻿/*
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
	using GreekAnalyzer = org.apache.lucene.analysis.el.GreekAnalyzer;

	using AnalyzerProvider = Org.Neo4j.Graphdb.index.fulltext.AnalyzerProvider;
	using Service = Org.Neo4j.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(AnalyzerProvider.class) public class Greek extends org.neo4j.graphdb.index.fulltext.AnalyzerProvider
	public class Greek : AnalyzerProvider
	{
		 public Greek() : base("greek")
		 {
		 }

		 public override Analyzer CreateAnalyzer()
		 {
			  return new GreekAnalyzer();
		 }

		 public override string Description()
		 {
			  return "Greek analyzer with stemming and stop word filtering.";
		 }
	}

}