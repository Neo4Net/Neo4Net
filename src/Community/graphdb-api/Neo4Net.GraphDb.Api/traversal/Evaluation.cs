using System.Collections.Generic;

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
namespace Neo4Net.GraphDb.Traversal
{
	/// <summary>
	/// Outcome of <seealso cref="Evaluator.evaluate(org.Neo4Net.graphdb.Path)"/>. An evaluation
	/// can tell the traversal whether or not to continue down that
	/// <seealso cref="TraversalBranch"/> and whether or not to include a
	/// <seealso cref="TraversalBranch"/> in the result of a traversal.
	/// 
	/// @author Mattias Persson </summary>
	/// <seealso cref= Evaluator </seealso>
	public sealed class Evaluation
	{
		 public static readonly Evaluation IncludeAndContinue = new Evaluation( "IncludeAndContinue", InnerEnum.IncludeAndContinue, true, true );
		 public static readonly Evaluation IncludeAndPrune = new Evaluation( "IncludeAndPrune", InnerEnum.IncludeAndPrune, true, false );
		 public static readonly Evaluation ExcludeAndContinue = new Evaluation( "ExcludeAndContinue", InnerEnum.ExcludeAndContinue, false, true );
		 public static readonly Evaluation ExcludeAndPrune = new Evaluation( "ExcludeAndPrune", InnerEnum.ExcludeAndPrune, false, false );

		 private static readonly IList<Evaluation> valueList = new List<Evaluation>();

		 static Evaluation()
		 {
			 valueList.Add( IncludeAndContinue );
			 valueList.Add( IncludeAndPrune );
			 valueList.Add( ExcludeAndContinue );
			 valueList.Add( ExcludeAndPrune );
		 }

		 public enum InnerEnum
		 {
			 IncludeAndContinue,
			 IncludeAndPrune,
			 ExcludeAndContinue,
			 ExcludeAndPrune
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal Evaluation( string name, InnerEnum innerEnum, bool includes, bool continues )
		 {
			  this._includes = includes;
			  this._continues = continues;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <returns> whether or not the <seealso cref="TraversalBranch"/> this outcome was
		 /// generated for should be included in the traversal result. </returns>
		 public bool Includes()
		 {
			  return this._includes;
		 }

		 /// <returns> whether or not the traversal should continue down the
		 /// <seealso cref="TraversalBranch"/> this outcome was generator for. </returns>
		 public bool Continues()
		 {
			  return _continues;
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluation"/> for the given {@code includes} and
		 /// {@code continues}.
		 /// </summary>
		 /// <param name="includes"> whether or not to include the <seealso cref="TraversalBranch"/>
		 /// in the traversal result. </param>
		 /// <param name="continues"> whether or not to continue down the
		 /// <seealso cref="TraversalBranch"/>. </param>
		 /// <returns> an <seealso cref="Evaluation"/> representing {@code includes}
		 /// and {@code continues}. </returns>
		 public static Evaluation Of( bool includes, bool continues )
		 {
			  return includes ? ( continues ? INCLUDE_AND_CONTINUE : INCLUDE_AND_PRUNE ) : ( continues ? EXCLUDE_AND_CONTINUE : EXCLUDE_AND_PRUNE );
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluation"/> for the given {@code includes}, meaning
		 /// whether or not to include a <seealso cref="TraversalBranch"/> in the traversal
		 /// result or not. The returned evaluation will always return true
		 /// for <seealso cref="Evaluation.continues()"/>.
		 /// </summary>
		 /// <param name="includes"> whether or not to include a <seealso cref="TraversalBranch"/>
		 /// in the traversal result. </param>
		 /// <returns> an <seealso cref="Evaluation"/> representing whether or not to include
		 /// a <seealso cref="TraversalBranch"/> in the traversal result. </returns>
		 public static Evaluation OfIncludes( bool includes )
		 {
			  return includes ? INCLUDE_AND_CONTINUE : EXCLUDE_AND_CONTINUE;
		 }

		 /// <summary>
		 /// Returns an <seealso cref="Evaluation"/> for the given {@code continues}, meaning
		 /// whether or not to continue further down a <seealso cref="TraversalBranch"/> in the
		 /// traversal. The returned evaluation will always return true for
		 /// <seealso cref="Evaluation.includes()"/>.
		 /// </summary>
		 /// <param name="continues"> whether or not to continue further down a
		 ///            <seealso cref="TraversalBranch"/> in the traversal. </param>
		 /// <returns> an <seealso cref="Evaluation"/> representing whether or not to continue
		 ///         further down a <seealso cref="TraversalBranch"/> in the traversal. </returns>
		 public static Evaluation OfContinues( bool continues )
		 {
			  return continues ? INCLUDE_AND_CONTINUE : INCLUDE_AND_PRUNE;
		 }

		public static IList<Evaluation> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static Evaluation valueOf( string name )
		{
			foreach ( Evaluation enumInstance in Evaluation.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}