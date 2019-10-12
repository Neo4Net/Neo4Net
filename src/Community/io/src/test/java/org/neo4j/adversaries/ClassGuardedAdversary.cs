using System;
using System.Collections.Generic;

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
namespace Neo4Net.Adversaries
{

	/// <summary>
	/// An adversary that delegates failure injection only when invoked through certain call sites.
	/// For every potential failure injection the current stack trace (the elements of it) are analyzed
	/// and if there's a match with the specified victims then failure will be delegated to the actual
	/// <seealso cref="Adversary"/> underneath.
	/// </summary>
	public class ClassGuardedAdversary : StackTraceElementGuardedAdversary
	{

		 public ClassGuardedAdversary( Adversary @delegate, params Type[] victimClassSet ) : base( @delegate, new PredicateAnonymousInnerClass( victimClassSet ) )
		 {
		 }

		 private class PredicateAnonymousInnerClass : System.Predicate<StackTraceElement>
		 {
			 private Type[] _victimClassSet;

			 public PredicateAnonymousInnerClass( Type[] victimClassSet )
			 {
				 this._victimClassSet = victimClassSet;
			 }

			 private ISet<string> victimClasses = Stream.of( _victimClassSet ).map( Type.getName ).collect( toSet() );

			 public override bool test( StackTraceElement stackTraceElement )
			 {
				  return victimClasses.contains( stackTraceElement.ClassName );
			 }
		 }

		 /// <summary>
		 /// Specifies victims as arbitrary <seealso cref="StackTraceElement"/> <seealso cref="Predicate"/>.
		 /// </summary>
		 /// <param name="delegate"> <seealso cref="Adversary"/> to delegate calls to. </param>
		 /// <param name="victims"> arbitrary <seealso cref="Predicate"/> for <seealso cref="StackTraceElement"/> in the executing
		 /// thread and if any of the elements in the current stack trace matches then failure is injected. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public ClassGuardedAdversary(Adversary delegate, System.Predicate<StackTraceElement>... victims)
		 public ClassGuardedAdversary( Adversary @delegate, params System.Predicate<StackTraceElement>[] victims ) : base( @delegate, victims )
		 {
		 }
	}

}