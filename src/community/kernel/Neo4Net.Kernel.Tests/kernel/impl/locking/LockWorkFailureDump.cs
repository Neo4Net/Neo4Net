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
namespace Neo4Net.Kernel.impl.locking
{

	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Log = Neo4Net.Logging.Log;

	public class LockWorkFailureDump
	{
		 private readonly File _file;

		 public LockWorkFailureDump( File file )
		 {
			  this._file = file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.File dumpState(Locks lm, LockWorker... workers) throws java.io.IOException
		 public virtual File DumpState( Locks lm, params LockWorker[] workers )
		 {
			  FileStream @out = new FileStream( _file, false );
			  FormattedLogProvider logProvider = FormattedLogProvider.withoutAutoFlush().toOutputStream(@out);

			  try
			  {
					//  * locks held by the lock manager
					lm.Accept( new DumpLocksVisitor( logProvider.GetLog( typeof( LockWorkFailureDump ) ) ) );
					//  * rag manager state;
					//  * workers state
					Log log = logProvider.getLog( this.GetType() );
					foreach ( LockWorker worker in workers )
					{
						 // - what each is doing and have up to now
						 log.Info( "Worker %s", worker );
					}
					return _file;
			  }
			  finally
			  {
					@out.Flush();
					@out.Close();
			  }
		 }
	}

}