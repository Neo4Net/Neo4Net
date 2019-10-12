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
namespace Org.Neo4j.Graphdb.index.fulltext
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;

	using Service = Org.Neo4j.Helpers.Service;

	/// <summary>
	/// This is the base-class for all service-loadable factory classes, that build the Lucene Analyzer instances that are available to the fulltext schema index.
	/// The analyzer factory is referenced in the index configuration via its {@code analyzerName} and {@code alternativeNames} that are specific to the constructor
	/// of this base class. Sub-classes must have a public no-arg constructor such that they can be service-loaded.
	/// <para>
	/// Here is an example that implements an analyzer provider for the {@code SwedishAnalyzer} that comes built into Lucene:
	/// 
	/// <pre><code>
	/// public class Swedish extends AnalyzerProvider
	/// {
	///     public Swedish()
	///     {
	///         super( "swedish" );
	///     }
	/// 
	///     public Analyzer createAnalyzer()
	///     {
	///         return new SwedishAnalyzer();
	///     }
	/// }
	/// </code></pre>
	/// </para>
	/// <para>
	/// The {@code jar} that includes this implementation must also contain a {@code META-INF/services/org.neo4j.graphdb.index.fulltext.AnalyzerProvider} file,
	/// that contains the fully-qualified class names of all of the {@code AnalyzerProvider} implementations it contains.
	/// </para>
	/// </summary>
	public abstract class AnalyzerProvider : Service
	{
		 /// <summary>
		 /// Sub-classes MUST have a public no-arg constructor, and must call this super-constructor with the names it uses to identify itself.
		 /// <para>
		 /// Sub-classes should strive to make these names unique.
		 /// If the names are not unique among all analyzer providers on the class path, then the indexes may fail to load the correct analyzers that they are
		 /// configured with.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="analyzerName"> The name of this analyzer provider, which will be used for analyzer settings values for identifying which implementation to use. </param>
		 /// <param name="alternativeNames"> The alternative names that can also be used to identify this analyzer provider. </param>
		 public AnalyzerProvider( string analyzerName, params string[] alternativeNames ) : base( analyzerName, alternativeNames )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static AnalyzerProvider getProviderByName(String analyzerName) throws java.util.NoSuchElementException
		 public static AnalyzerProvider GetProviderByName( string analyzerName )
		 {
			  return Load( typeof( AnalyzerProvider ), analyzerName );
		 }

		 /// <returns> A newly constructed {@code Analyzer} instance. </returns>
		 public abstract Analyzer CreateAnalyzer();

		 /// <returns> A description of this analyzer. </returns>
		 public virtual string Description()
		 {
			  return "";
		 }
	}

}