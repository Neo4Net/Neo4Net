﻿using System;
using System.Threading;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

	/// <summary>
	/// Allocates its own <seealso cref="InputChunk"/> and loops, getting input data, importing input data into store
	/// until no more chunks are available.
	/// </summary>
	internal class ExhaustingEntityImporterRunnable : ThreadStart
	{
		 private readonly InputIterator _data;
		 private readonly IEntityImporter _visitor;
		 private readonly LongAdder _roughEntityCountProgress;
		 private readonly StageControl _control;

		 internal ExhaustingEntityImporterRunnable( StageControl control, InputIterator data, IEntityImporter visitor, LongAdder roughEntityCountProgress )
		 {
			  this._control = control;
			  this._data = data;
			  this._visitor = visitor;
			  this._roughEntityCountProgress = roughEntityCountProgress;
		 }

		 public override void Run()
		 {
			  try
			  {
					  using ( InputChunk chunk = _data.newChunk() )
					  {
						while ( _data.next( chunk ) )
						{
							 _control.assertHealthy();
							 int count = 0;
							 while ( chunk.Next( _visitor ) )
							 {
								  count++;
							 }
							 _roughEntityCountProgress.add( count );
						}
					  }
			  }
			  catch ( IOException e )
			  {
					_control.panic( e );
					throw new Exception( e );
			  }
			  catch ( Exception e )
			  {
					_control.panic( e );
					throw e;
			  }
			  finally
			  {
					_visitor.Dispose();
			  }
		 }
	}

}