using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.proc.temporal
{

	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using FieldSignature = Neo4Net.Internal.Kernel.Api.procs.FieldSignature;
	using Neo4NetTypes = Neo4Net.Internal.Kernel.Api.procs.Neo4NetTypes;
	using QualifiedName = Neo4Net.Internal.Kernel.Api.procs.QualifiedName;
	using UserFunctionSignature = Neo4Net.Internal.Kernel.Api.procs.UserFunctionSignature;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Description = Neo4Net.Procedure.Description;
	using AnyValue = Neo4Net.Values.AnyValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.procs.FieldSignature.inputField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	[Description("Construct a Duration value.")]
	internal class DurationFunction : CallableUserFunction
	{
		 private static readonly UserFunctionSignature _duration = new UserFunctionSignature( new QualifiedName( new string[0], "duration" ), Collections.singletonList( inputField( "input", Neo4NetTypes.NTAny ) ), Neo4NetTypes.NTDuration, null, new string[0], typeof( DurationFunction ).getAnnotation( typeof( Description ) ).value(), true );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void register(org.Neo4Net.kernel.impl.proc.Procedures procedures) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 internal static void Register( Procedures procedures )
		 {
			  procedures.Register( new DurationFunction() );
			  procedures.Register( new Between( "between" ) );
			  procedures.Register( new Between( "inMonths" ) );
			  procedures.Register( new Between( "inDays" ) );
			  procedures.Register( new Between( "inSeconds" ) );
		 }

		 public override UserFunctionSignature Signature()
		 {
			  return _duration;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue apply(org.Neo4Net.kernel.api.proc.Context ctx, org.Neo4Net.values.AnyValue[] input) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public override AnyValue Apply( Context ctx, AnyValue[] input )
		 {
			  if ( input == null )
			  {
					return NO_VALUE;
			  }
			  else if ( input.Length == 1 )
			  {
					if ( input[0] == NO_VALUE || input[0] == null )
					{
						 return NO_VALUE;
					}
					else if ( input[0] is TextValue )
					{
						 return DurationValue.parse( ( TextValue ) input[0] );
					}
					else if ( input[0] is MapValue )
					{
						 MapValue map = ( MapValue ) input[0];
						 return DurationValue.build( map );
					}
			  }
			  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
		 }

		 private class Between : CallableUserFunction
		 {
			  internal const string DESCRIPTION = "Compute the duration between the 'from' instant (inclusive) and the 'to' instant (exclusive) in %s.";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal static readonly IList<FieldSignature> SignatureConflict = Arrays.asList( inputField( "from", Neo4NetTypes.NTAny ), inputField( "to", Neo4NetTypes.NTAny ) );
			  internal readonly UserFunctionSignature Signature;
			  internal readonly TemporalUnit Unit;

			  internal Between( string unit )
			  {
					string unitString;
					switch ( unit )
					{
					case "between":
						 this.Unit = null;
						 unitString = "logical units";
						 break;
					case "inMonths":
						 this.Unit = ChronoUnit.MONTHS;
						 unitString = "months";
						 break;
					case "inDays":
						 this.Unit = ChronoUnit.DAYS;
						 unitString = "days";
						 break;
					case "inSeconds":
						 this.Unit = ChronoUnit.SECONDS;
						 unitString = "seconds";
						 break;
					default:
						 throw new System.InvalidOperationException( "Unsupported unit: " + unit );
					}
					this.Signature = new UserFunctionSignature( new QualifiedName( new string[] { "duration" }, unit ), SignatureConflict, Neo4NetTypes.NTDuration, null, new string[0], string.format( DESCRIPTION, unitString ), true );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return Signature;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue apply(org.Neo4Net.kernel.api.proc.Context ctx, org.Neo4Net.values.AnyValue[] input) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
			  public override AnyValue Apply( Context ctx, AnyValue[] input )
			  {
					if ( input == null || ( input.Length == 2 && ( input[0] == NO_VALUE || input[0] == null ) || input[1] == NO_VALUE || input[1] == null ) )
					{
						 return NO_VALUE;
					}
					else if ( input.Length == 2 )
					{
						 if ( input[0] is TemporalValue && input[1] is TemporalValue )
						 {
							  TemporalValue from = ( TemporalValue ) input[0];
							  TemporalValue to = ( TemporalValue ) input[1];
							  return DurationValue.between( Unit, from, to );
						 }
					}
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
			  }
		 }
	}

}