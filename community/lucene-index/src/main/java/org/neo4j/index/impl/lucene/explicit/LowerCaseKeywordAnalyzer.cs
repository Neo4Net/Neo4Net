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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using KeywordTokenizer = org.apache.lucene.analysis.core.KeywordTokenizer;
	using LowerCaseFilter = org.apache.lucene.analysis.core.LowerCaseFilter;

	public sealed class LowerCaseKeywordAnalyzer : Analyzer
	{
		 protected internal override TokenStreamComponents CreateComponents( string fieldName )
		 {
			  KeywordTokenizer source = new KeywordTokenizer();
			  return new TokenStreamComponents( source, new LowerCaseFilter( source ) );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }
	}

}