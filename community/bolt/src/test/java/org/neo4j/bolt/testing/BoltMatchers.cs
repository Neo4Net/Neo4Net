using System;

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
namespace Org.Neo4j.Bolt.testing
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;


	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineState = Org.Neo4j.Bolt.runtime.BoltStateMachineState;
	using StatementProcessor = Org.Neo4j.Bolt.runtime.StatementProcessor;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using BoltStateMachineV1 = Org.Neo4j.Bolt.v1.runtime.BoltStateMachineV1;
	using ReadyState = Org.Neo4j.Bolt.v1.runtime.ReadyState;
	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;
	using Org.Neo4j.Function;
	using Org.Neo4j.Function;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.IGNORED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.MachineRoom.newMachine;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class BoltMatchers
	{
		 private BoltMatchers()
		 {
		 }

		 public static Matcher<RecordedBoltResponse> Succeeded()
		 {
			  return new BaseMatcherAnonymousInnerClass();
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<RecordedBoltResponse>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordedBoltResponse response = (RecordedBoltResponse) item;
				  RecordedBoltResponse response = ( RecordedBoltResponse ) item;
				  return response.Message() == SUCCESS;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( SUCCESS );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<RecordedBoltResponse> succeededWithMetadata(final String key, final String value)
		 public static Matcher<RecordedBoltResponse> SucceededWithMetadata( string key, string value )
		 {
			  return succeededWithMetadata( key, stringValue( value ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<RecordedBoltResponse> succeededWithMetadata(final String key, final org.neo4j.values.AnyValue value)
		 public static Matcher<RecordedBoltResponse> SucceededWithMetadata( string key, AnyValue value )
		 {
			  return new BaseMatcherAnonymousInnerClass2( key, value );
		 }

		 private class BaseMatcherAnonymousInnerClass2 : BaseMatcher<RecordedBoltResponse>
		 {
			 private string _key;
			 private AnyValue _value;

			 public BaseMatcherAnonymousInnerClass2( string key, AnyValue value )
			 {
				 this._key = key;
				 this._value = value;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordedBoltResponse response = (RecordedBoltResponse) item;
				  RecordedBoltResponse response = ( RecordedBoltResponse ) item;
				  return response.Message() == SUCCESS && response.HasMetadata(_key) && response.Metadata(_key).Equals(_value);
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( SUCCESS ).appendText( format( " with metadata %s = %s", _key, _value.ToString() ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<RecordedBoltResponse> containsRecord(final Object... values)
		 public static Matcher<RecordedBoltResponse> ContainsRecord( params object[] values )
		 {
			  return new BaseMatcherAnonymousInnerClass3( values );
		 }

		 private class BaseMatcherAnonymousInnerClass3 : BaseMatcher<RecordedBoltResponse>
		 {
			 private object[] _values;

			 public BaseMatcherAnonymousInnerClass3( object[] values )
			 {
				 this._values = values;
			 }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			 private AnyValue[] anyValues = Arrays.stream( _values ).map( ValueUtils.of ).toArray( AnyValue[]::new );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordedBoltResponse response = (RecordedBoltResponse) item;
				  RecordedBoltResponse response = ( RecordedBoltResponse ) item;
				  Org.Neo4j.Cypher.result.QueryResult_Record[] records = response.Records();
				  return records.Length > 0 && Arrays.Equals( records[0].Fields(), anyValues );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( format( "with record %s", _values ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<RecordedBoltResponse> succeededWithRecord(final Object... values)
		 public static Matcher<RecordedBoltResponse> SucceededWithRecord( params object[] values )
		 {
			  return new BaseMatcherAnonymousInnerClass4( values );
		 }

		 private class BaseMatcherAnonymousInnerClass4 : BaseMatcher<RecordedBoltResponse>
		 {
			 private object[] _values;

			 public BaseMatcherAnonymousInnerClass4( object[] values )
			 {
				 this._values = values;
			 }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			 private AnyValue[] anyValues = Arrays.stream( _values ).map( ValueUtils.of ).toArray( AnyValue[]::new );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordedBoltResponse response = (RecordedBoltResponse) item;
				  RecordedBoltResponse response = ( RecordedBoltResponse ) item;
				  Org.Neo4j.Cypher.result.QueryResult_Record[] records = response.Records();
				  return response.Message() == SUCCESS && Arrays.Equals(records[0].Fields(), anyValues);
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( SUCCESS ).appendText( format( " with record %s", _values ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<RecordedBoltResponse> succeededWithMetadata(final String key, final java.util.regex.Pattern pattern)
		 public static Matcher<RecordedBoltResponse> SucceededWithMetadata( string key, Pattern pattern )
		 {
			  return new BaseMatcherAnonymousInnerClass5( key, pattern );
		 }

		 private class BaseMatcherAnonymousInnerClass5 : BaseMatcher<RecordedBoltResponse>
		 {
			 private string _key;
			 private Pattern _pattern;

			 public BaseMatcherAnonymousInnerClass5( string key, Pattern pattern )
			 {
				 this._key = key;
				 this._pattern = pattern;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordedBoltResponse response = (RecordedBoltResponse) item;
				  RecordedBoltResponse response = ( RecordedBoltResponse ) item;
				  return response.Message() == SUCCESS && response.HasMetadata(_key) && _pattern.matcher(((TextValue) response.Metadata(_key)).stringValue()).matches();
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( SUCCESS ).appendText( format( " with metadata %s ~ %s", _key, _pattern.ToString() ) );
			 }
		 }

		 public static Matcher<RecordedBoltResponse> WasIgnored()
		 {
			  return new BaseMatcherAnonymousInnerClass6();
		 }

		 private class BaseMatcherAnonymousInnerClass6 : BaseMatcher<RecordedBoltResponse>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordedBoltResponse response = (RecordedBoltResponse) item;
				  RecordedBoltResponse response = ( RecordedBoltResponse ) item;
				  return response.Message() == IGNORED;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( IGNORED );
			 }
		 }

		 public static Matcher<RecordedBoltResponse> FailedWithStatus( Status status )
		 {
			  return new BaseMatcherAnonymousInnerClass7( status );
		 }

		 private class BaseMatcherAnonymousInnerClass7 : BaseMatcher<RecordedBoltResponse>
		 {
			 private Status _status;

			 public BaseMatcherAnonymousInnerClass7( Status status )
			 {
				 this._status = status;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RecordedBoltResponse response = (RecordedBoltResponse) item;
				  RecordedBoltResponse response = ( RecordedBoltResponse ) item;
				  return response.Message() == FAILURE && response.HasMetadata("code") && response.Metadata("code").Equals(stringValue(_status.code().serialize()));
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( FAILURE ).appendText( format( " with status code %s", _status.code().serialize() ) );
			 }
		 }

		 public static Matcher<BoltStateMachine> HasTransaction()
		 {
			  return new BaseMatcherAnonymousInnerClass8();
		 }

		 private class BaseMatcherAnonymousInnerClass8 : BaseMatcher<BoltStateMachine>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.runtime.BoltStateMachineV1 machine = (org.neo4j.bolt.v1.runtime.BoltStateMachineV1) item;
				  BoltStateMachineV1 machine = ( BoltStateMachineV1 ) item;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.StatementProcessor statementProcessor = machine.statementProcessor();
				  StatementProcessor statementProcessor = machine.StatementProcessor();
				  return statementProcessor != null && statementProcessor.HasTransaction();
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "no transaction" );
			 }
		 }

		 public static Matcher<BoltStateMachine> HasNoTransaction()
		 {
			  return new BaseMatcherAnonymousInnerClass9();
		 }

		 private class BaseMatcherAnonymousInnerClass9 : BaseMatcher<BoltStateMachine>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.runtime.BoltStateMachineV1 machine = (org.neo4j.bolt.v1.runtime.BoltStateMachineV1) item;
				  BoltStateMachineV1 machine = ( BoltStateMachineV1 ) item;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.StatementProcessor statementProcessor = machine.statementProcessor();
				  StatementProcessor statementProcessor = machine.StatementProcessor();
				  return statementProcessor == null || !statementProcessor.HasTransaction();
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "no transaction" );
			 }
		 }

		 public static Matcher<BoltStateMachine> InState( Type stateClass )
		 {
			  return new BaseMatcherAnonymousInnerClass10( stateClass );
		 }

		 private class BaseMatcherAnonymousInnerClass10 : BaseMatcher<BoltStateMachine>
		 {
			 private Type _stateClass;

			 public BaseMatcherAnonymousInnerClass10( Type stateClass )
			 {
				 this._stateClass = stateClass;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
				  return _stateClass.IsInstanceOfType( ( ( BoltStateMachineV1 ) item ).state() );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "can reset" );
			 }
		 }

		 public static Matcher<BoltStateMachine> Closed
		 {
			 get
			 {
				  return new BaseMatcherAnonymousInnerClass11();
			 }
		 }

		 private class BaseMatcherAnonymousInnerClass11 : BaseMatcher<BoltStateMachine>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = (org.neo4j.bolt.runtime.BoltStateMachine) item;
				  BoltStateMachine machine = ( BoltStateMachine ) item;
				  return machine.Closed;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "can reset" );
			 }
		 }

		 public static Matcher<BoltStateMachine> CanReset()
		 {
			  return new BaseMatcherAnonymousInnerClass12();
		 }

		 private class BaseMatcherAnonymousInnerClass12 : BaseMatcher<BoltStateMachine>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = (org.neo4j.bolt.runtime.BoltStateMachine) item;
				  BoltStateMachine machine = ( BoltStateMachine ) item;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final BoltResponseRecorder recorder = new BoltResponseRecorder();
				  BoltResponseRecorder recorder = new BoltResponseRecorder();
				  try
				  {
						machine.Process( ResetMessage.INSTANCE, recorder );
						return recorder.ResponseCount() == 1 && InState(typeof(ReadyState)).matches(item);
				  }
				  catch ( BoltConnectionFatality )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "can reset" );
			 }
		 }

		 public static void VerifyKillsConnection( ThrowingAction<BoltConnectionFatality> action )
		 {
			  try
			  {
					action.Apply();
					fail( "should have killed the connection" );
			  }
			  catch ( BoltConnectionFatality )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void verifyOneResponse(org.neo4j.function.ThrowingBiConsumer<org.neo4j.bolt.runtime.BoltStateMachine,BoltResponseRecorder,org.neo4j.bolt.runtime.BoltConnectionFatality> transition) throws Exception
		 public static void VerifyOneResponse( ThrowingBiConsumer<BoltStateMachine, BoltResponseRecorder, BoltConnectionFatality> transition )
		 {
			  BoltStateMachine machine = newMachine();
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  try
			  {
					transition.Accept( machine, recorder );
			  }
			  catch ( BoltConnectionFatality )
			  {
					// acceptable for invalid transitions
			  }
			  assertEquals( 1, recorder.ResponseCount() );
		 }
	}

}