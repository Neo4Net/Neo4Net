using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Exceptions.throwIfUnchecked;

	/// <summary>
	/// A simple <seealso cref="StageControl"/> for tests with multiple steps and where an error or assertion failure
	/// propagates to other steps. Create the <seealso cref="SimpleStageControl"/>, pass it into the <seealso cref="Step steps"/>
	/// and then when all steps are created, call <seealso cref="steps(Step...)"/> to let the control know about them.
	/// </summary>
	public class SimpleStageControl : StageControl
	{
		 private volatile Exception _panic;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private volatile Step<?>[] steps;
		 private volatile Step<object>[] _steps;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public void steps(Step<?>... steps)
		 public virtual void Steps( params Step<object>[] steps )
		 {
			  this._steps = steps;
		 }

		 public override void Panic( Exception cause )
		 {
			  this._panic = cause;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : steps)
			  foreach ( Step<object> step in _steps )
			  {
					step.ReceivePanic( cause );
					step.EndOfUpstream();
			  }
		 }

		 public override void AssertHealthy()
		 {
			  if ( _panic != null )
			  {
					throwIfUnchecked( _panic );
					throw new Exception( _panic );
			  }
		 }

		 public override void Recycle( object batch )
		 {
		 }

		 public override T Reuse<T>( System.Func<T> fallback )
		 {
			  return fallback();
		 }
	}

}