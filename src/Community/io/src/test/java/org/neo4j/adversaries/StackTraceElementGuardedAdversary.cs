using System;
using System.Threading;

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

	public class StackTraceElementGuardedAdversary : Adversary
	{
		 private readonly Adversary @delegate;
		 private readonly System.Predicate<StackTraceElement>[] _checks;
		 private volatile bool _enabled;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public StackTraceElementGuardedAdversary(Adversary delegate, System.Predicate<StackTraceElement>... checks)
		 public StackTraceElementGuardedAdversary( Adversary @delegate, params System.Predicate<StackTraceElement>[] checks )
		 {
			  this.@delegate = @delegate;
			  this._checks = checks;
			  _enabled = true;
		 }

		 public override void InjectFailure( params Type[] failureTypes )
		 {
			  if ( _enabled && CalledFromVictimStackTraceElement() )
			  {
					DelegateFailureInjection( failureTypes );
			  }
		 }

		 public override bool InjectFailureOrMischief( params Type[] failureTypes )
		 {
			  return _enabled && CalledFromVictimStackTraceElement() && DelegateFailureOrMischiefInjection(failureTypes);
		 }

		 protected internal virtual void DelegateFailureInjection( Type[] failureTypes )
		 {
			  @delegate.InjectFailure( failureTypes );
		 }

		 protected internal virtual bool DelegateFailureOrMischiefInjection( Type[] failureTypes )
		 {
			  return @delegate.InjectFailureOrMischief( failureTypes );
		 }

		 private bool CalledFromVictimStackTraceElement()
		 {
			  StackTraceElement[] stackTrace = Thread.CurrentThread.StackTrace;
			  foreach ( StackTraceElement element in stackTrace )
			  {
					foreach ( System.Predicate<StackTraceElement> check in _checks )
					{
						 if ( check( element ) )
						 {
							  return true;
						 }
					}

			  }
			  return false;
		 }

		 public virtual void Disable()
		 {
			  _enabled = false;
		 }

		 public virtual void Enable()
		 {
			  _enabled = true;
		 }
	}

}