using System.Collections.Generic;
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
namespace Neo4Net.Server
{

	public class BlockingBootstrapper : Bootstrapper
	{
		 private readonly Bootstrapper _wrapped;
		 private readonly System.Threading.CountdownEvent _latch;

		 public BlockingBootstrapper( Bootstrapper wrapped )
		 {
			  this._wrapped = wrapped;
			  this._latch = new System.Threading.CountdownEvent( 1 );
		 }

		 public override int Start( File homeDir, Optional<File> configFile, IDictionary<string, string> configOverrides )
		 {
			  int status = _wrapped.start( homeDir, configFile, configOverrides );
			  if ( status != ServerBootstrapper.OK )
			  {
					return status;
			  }

			  try
			  {
					_latch.await();
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
			  }

			  return _wrapped.stop();
		 }

		 public override int Stop()
		 {
			  _latch.Signal();
			  return 0;
		 }
	}

}