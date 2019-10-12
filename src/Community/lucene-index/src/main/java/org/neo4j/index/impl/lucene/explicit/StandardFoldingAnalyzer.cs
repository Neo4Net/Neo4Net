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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using TokenStream = org.apache.lucene.analysis.TokenStream;
	using LowerCaseFilter = org.apache.lucene.analysis.core.LowerCaseFilter;
	using StopAnalyzer = org.apache.lucene.analysis.core.StopAnalyzer;
	using StopFilter = org.apache.lucene.analysis.core.StopFilter;
	using ASCIIFoldingFilter = org.apache.lucene.analysis.miscellaneous.ASCIIFoldingFilter;
	using StandardFilter = org.apache.lucene.analysis.standard.StandardFilter;
	using StandardTokenizer = org.apache.lucene.analysis.standard.StandardTokenizer;
	using StopwordAnalyzerBase = org.apache.lucene.analysis.util.StopwordAnalyzerBase;

	/// <summary>
	/// Analyzer that uses ASCIIFoldingFilter to remove accents (diacritics).
	/// Otherwise behaves as standard english analyzer.
	/// 
	/// Implementation inspired by org.apache.lucene.analysis.standard.StandardAnalyzer
	/// </summary>
	public sealed class StandardFoldingAnalyzer : StopwordAnalyzerBase
	{

		 /// <summary>
		 /// Default maximum allowed token length </summary>
		 public const int DEFAULT_MAX_TOKEN_LENGTH = 255;

		 public StandardFoldingAnalyzer() : base(StopAnalyzer.ENGLISH_STOP_WORDS_SET)
		 {
		 }

		 protected internal override TokenStreamComponents CreateComponents( string fieldName )
		 {
			  StandardTokenizer src = new StandardTokenizer();
			  src.MaxTokenLength = DEFAULT_MAX_TOKEN_LENGTH;
			  TokenStream tok = new StandardFilter( src );
			  tok = new LowerCaseFilter( tok );
			  tok = new StopFilter( tok, stopwords );
			  tok = new ASCIIFoldingFilter( tok );
			  return new TokenStreamComponents( src, tok );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }
	}

}