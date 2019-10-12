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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	/// <summary>
	/// A stage of processing, mainly consisting of one or more <seealso cref="Step steps"/> that batches of data to
	/// process flows through.
	/// </summary>
	public class Stage
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<Step<?>> pipeline = new java.util.ArrayList<>();
		 private readonly IList<Step<object>> _pipeline = new List<Step<object>>();
		 private readonly StageExecution _execution;

		 public Stage( string name, string part, Configuration config, int orderingGuarantees )
		 {
			  this._execution = new StageExecution( name, part, config, _pipeline, orderingGuarantees );
		 }

		 protected internal virtual StageControl Control()
		 {
			  return _execution;
		 }

		 public virtual void Add<T1>( Step<T1> step )
		 {
			  _pipeline.Add( step );
		 }

		 public virtual StageExecution Execute()
		 {
			  LinkSteps();
			  _execution.start();
			  _pipeline[0].receive( 1, null );
			  return _execution;
		 }

		 private void LinkSteps()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> previous = null;
			  Step<object> previous = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : pipeline)
			  foreach ( Step<object> step in _pipeline )
			  {
					if ( previous != null )
					{
						 previous.Downstream = step;
					}
					previous = step;
			  }
		 }

		 public virtual void Close()
		 {
			  Exception exception = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : pipeline)
			  foreach ( Step<object> step in _pipeline )
			  {
					try
					{
						 step.Close();
					}
					catch ( Exception e )
					{
						 if ( exception == null )
						 {
							  exception = e;
						 }
						 else
						 {
							  exception.addSuppressed( e );
						 }
					}
			  }
			  _execution.close();
			  if ( exception != null )
			  {
					throw new Exception( exception );
			  }
		 }

		 public override string ToString()
		 {
			  return _execution.StageName;
		 }
	}

}