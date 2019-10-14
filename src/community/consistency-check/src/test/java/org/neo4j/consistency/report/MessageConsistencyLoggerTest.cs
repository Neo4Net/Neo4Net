using System;
using System.Text;

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
namespace Neo4Net.Consistency.report
{
	using Test = org.junit.jupiter.api.Test;

	using Strings = Neo4Net.Helpers.Strings;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

	internal class MessageConsistencyLoggerTest
	{
		private bool InstanceFieldsInitialized = false;

		public MessageConsistencyLoggerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_logger = new InconsistencyMessageLogger( _logProvider.getLog( this.GetType() ) );
		}

		 private static readonly AssertableLogProvider.LogMatcherBuilder _inlog = AssertableLogProvider.inLog( typeof( MessageConsistencyLoggerTest ) );
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private InconsistencyMessageLogger _logger;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFormatErrorForRecord()
		 internal virtual void ShouldFormatErrorForRecord()
		 {
			  // when
			  _logger.error( RecordType.NEO_STORE, new NeoStoreRecord(), "sample message", 1, 2 );

			  // then
			  _logProvider.assertExactly( _inlog.error( Join( "sample message", NeoStoreRecord( true, -1 ), "Inconsistent with: 1 2" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFlattenAMultiLineMessageToASingleLine()
		 internal virtual void ShouldFlattenAMultiLineMessageToASingleLine()
		 {
			  // when
			  _logger.error( RecordType.NEO_STORE, new NeoStoreRecord(), "multiple\n line\r\n message", 1, 2 );

			  // then
			  _logProvider.assertExactly( _inlog.error( Join( "multiple line message", NeoStoreRecord( true, -1 ), "Inconsistent with: 1 2" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFormatWarningForRecord()
		 internal virtual void ShouldFormatWarningForRecord()
		 {
			  // when
			  _logger.warning( RecordType.NEO_STORE, new NeoStoreRecord(), "sample message", 1, 2 );

			  // then
			  _logProvider.assertExactly( _inlog.warn( Join( "sample message", NeoStoreRecord( true, -1 ), "Inconsistent with: 1 2" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFormatLogForChangedRecord()
		 internal virtual void ShouldFormatLogForChangedRecord()
		 {
			  // when
			  _logger.error( RecordType.NEO_STORE, new NeoStoreRecord(), new NeoStoreRecord(), "sample message", 1, 2 );

			  // then
			  _logProvider.assertExactly( _inlog.error( Join( "sample message", "- " + NeoStoreRecord( true, -1 ), "+ " + NeoStoreRecord( true, -1 ), "Inconsistent with: 1 2" ) ) );
		 }

		 private static string Join( string firstLine, params string[] lines )
		 {
			  StringBuilder expected = new StringBuilder( firstLine );
			  foreach ( string line in lines )
			  {
					expected.Append( Environment.NewLine ).Append( Strings.TAB ).Append( line );
			  }
			  return expected.ToString();
		 }

		 private static string NeoStoreRecord( bool used, long nextProp )
		 {
			  NeoStoreRecord record = new NeoStoreRecord();
			  record.InUse = used;
			  record.NextProp = nextProp;
			  return record.ToString();
		 }
	}

}