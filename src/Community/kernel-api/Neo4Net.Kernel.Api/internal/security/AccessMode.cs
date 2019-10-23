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
namespace Neo4Net.Kernel.Api.Internal.security
{
	using AuthorizationViolationException = Neo4Net.GraphDb.security.AuthorizationViolationException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Controls the capabilities of a KernelTransaction. </summary>
	public interface AccessMode
	{

		 bool AllowsReads();
		 bool AllowsWrites();
		 bool AllowsTokenCreates();
		 bool AllowsSchemaWrites();

		 bool AllowsPropertyReads( int propertyKey );

		 /// <summary>
		 /// Determines whether this mode allows execution of a procedure with the parameter string array in its
		 /// procedure annotation.
		 /// </summary>
		 /// <param name="allowed"> An array of strings that encodes permissions that allows the execution of a procedure </param>
		 /// <returns> {@code true} if this mode allows the execution of a procedure with the given parameter string array
		 /// encoding permission </returns>
		 bool AllowsProcedureWith( string[] allowed );

		 AuthorizationViolationException OnViolation( string msg );
		 string Name();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean isOverridden()
	//	 {
	//		  return false;
	//	 }
	}

	 public sealed class AccessMode_Static : AccessMode
	 {
		  /// <summary>
		  /// No reading or writing allowed. </summary>
		  public static readonly AccessMode_Static None = new AccessMode_Static( "None", InnerEnum.None, false, false, false, false, false, false );
		  /// <summary>
		  /// No reading or writing allowed because of expired credentials. </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//        CREDENTIALS_EXPIRED(false, false, false, false, false, false) { public org.Neo4Net.graphdb.security.AuthorizationViolationException onViolation(String msg) { return new org.Neo4Net.graphdb.security.AuthorizationViolationException(String.format(msg + "%n%nThe credentials you provided were valid, but must be " + "changed before you can " + "use this instance. If this is the first time you are using Neo4Net, this is to " + "ensure you are not using the default credentials in production. If you are not " + "using default credentials, you are getting this message because an administrator " + "requires a password change.%n" + "Changing your password is easy to do via the Neo4Net Browser.%n" + "If you are connecting via a shell or programmatically via a driver, " + "just issue a `CALL dbms.changePassword('new password')` statement in the current " + "session, and then restart your driver with the new password configured."), org.Neo4Net.kernel.api.exceptions.Status_Security.CredentialsExpired); } },

		  /// <summary>
		  /// Allows reading data and schema, but not writing. </summary>
		  public static readonly AccessMode_Static Read = new AccessMode_Static( "Read", InnerEnum.Read, true, false, false, false, false, true );
		  /// <summary>
		  /// Allows writing data </summary>
		  public static readonly AccessMode_Static WriteOnly = new AccessMode_Static( "WriteOnly", InnerEnum.WriteOnly, false, true, false, false, false, true );
		  /// <summary>
		  /// Allows reading and writing data, but not schema. </summary>
		  public static readonly AccessMode_Static Write = new AccessMode_Static( "Write", InnerEnum.Write, true, true, false, false, false, true );
		  /// <summary>
		  /// Allows reading and writing data and creating new tokens, but not schema. </summary>
		  public static readonly AccessMode_Static TokenWrite = new AccessMode_Static( "TokenWrite", InnerEnum.TokenWrite, true, true, true, false, false, true );
		  /// <summary>
		  /// Allows all operations. </summary>
		  public static readonly AccessMode_Static Full = new AccessMode_Static( "Full", InnerEnum.Full, true, true, true, true, true, true );

		  private static readonly IList<AccessMode_Static> valueList = new List<AccessMode_Static>();

		  static AccessMode_Static()
		  {
			  valueList.Add( None );
			  valueList.Add( CREDENTIALS_EXPIRED );
			  valueList.Add( Read );
			  valueList.Add( WriteOnly );
			  valueList.Add( Write );
			  valueList.Add( TokenWrite );
			  valueList.Add( Full );
		  }

		  public enum InnerEnum
		  {
			  None,
			  CREDENTIALS_EXPIRED,
			  Read,
			  WriteOnly,
			  Write,
			  TokenWrite,
			  Full
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final boolean;
		  internal Final boolean;
		  internal Final boolean;
		  internal Final boolean;
		  internal Final boolean;
		  internal Final boolean;

		  internal AccessMode_Static( string name, InnerEnum innerEnum, bool read, bool write, bool token, bool schema, bool procedure, bool property )
		  {
				this.Read = read;
				this.Write = write;
				this.Token = token;
				this.Schema = schema;
				this.Procedure = procedure;
				this.Property = property;

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public bool AllowsReads()
		  {
				return Read;
		  }

		  public bool AllowsWrites()
		  {
				return Write;
		  }

		  public bool AllowsTokenCreates()
		  {
				return Token;
		  }

		  public bool AllowsSchemaWrites()
		  {
				return Schema;
		  }

		  public bool AllowsPropertyReads( int propertyKey )
		  {
				return Property;
		  }

		  public bool AllowsProcedureWith( string[] allowed )
		  {
				return Procedure;
		  }

		  public Neo4Net.GraphDb.security.AuthorizationViolationException OnViolation( string msg )
		  {
				return new AuthorizationViolationException( msg );
		  }

		 public static IList<AccessMode_Static> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static AccessMode_Static ValueOf( string name )
		 {
			 foreach ( AccessMode_Static enumInstance in AccessMode_Static.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

}