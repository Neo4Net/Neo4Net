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
namespace Neo4Net.Kernel.impl.util.watcher
{
	using Test = org.junit.Test;

	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using SimpleLogService = Neo4Net.Logging.@internal.SimpleLogService;

	public class DefaultFileDeletionEventListenerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notificationInLogAboutFileDeletion()
		 public virtual void NotificationInLogAboutFileDeletion()
		 {
			  AssertableLogProvider internalLogProvider = new AssertableLogProvider( false );
			  DefaultFileDeletionEventListener listener = BuildListener( internalLogProvider );
			  listener.FileDeleted( "testFile.db" );
			  listener.FileDeleted( "anotherDirectory" );

			  internalLogProvider.RawMessageMatcher().assertContains("'testFile.db' which belongs to the store was deleted while database was running.");
			  internalLogProvider.RawMessageMatcher().assertContains("'anotherDirectory' which belongs to the store was deleted while database was running.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noNotificationForTransactionLogs()
		 public virtual void NoNotificationForTransactionLogs()
		 {
			  AssertableLogProvider internalLogProvider = new AssertableLogProvider( false );
			  DefaultFileDeletionEventListener listener = BuildListener( internalLogProvider );
			  listener.FileDeleted( TransactionLogFiles.DEFAULT_NAME + ".0" );
			  listener.FileDeleted( TransactionLogFiles.DEFAULT_NAME + ".1" );

			  internalLogProvider.AssertNoLoggingOccurred();
		 }

		 private DefaultFileDeletionEventListener BuildListener( AssertableLogProvider internalLogProvider )
		 {
			  SimpleLogService logService = new SimpleLogService( NullLogProvider.Instance, internalLogProvider );
			  return new DefaultFileDeletionEventListener( logService, filename => filename.StartsWith( TransactionLogFiles.DEFAULT_NAME ) );
		 }
	}

}