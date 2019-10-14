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
namespace Neo4Net.Ssl
{

	/// <summary>
	/// Client side modifier for SSLEngine to mandate hostname verification
	/// </summary>
	public class ClientSideHostnameVerificationEngineModification : System.Func<SSLEngine, SSLEngine>
	{
		 /// <summary>
		 /// Apply modifications to engine to enable hostname verification (client side only)
		 /// </summary>
		 /// <param name="sslEngine"> the engine used for handling TLS. Will be mutated by this method </param>
		 /// <returns> the updated sslEngine that allows client side hostname verification </returns>
		 public override SSLEngine Apply( SSLEngine sslEngine )
		 {
			  SSLParameters sslParameters = sslEngine.SSLParameters;
			  sslParameters.EndpointIdentificationAlgorithm = VerificationAlgorithm.Https.Value;
			  sslEngine.SSLParameters = sslParameters;
			  return sslEngine;
		 }

		 private sealed class VerificationAlgorithm
		 {
			  /*
			  Endpoint identification algorithms
			  HTTPS http://www.ietf.org/rfc/rfc2818.txt
			  LDAPS http://www.ietf.org/rfc/rfc2830.txt
			   */
			  public static readonly VerificationAlgorithm Https = new VerificationAlgorithm( "Https", InnerEnum.Https, "HTTPS" );
			  public static readonly VerificationAlgorithm Ldaps = new VerificationAlgorithm( "Ldaps", InnerEnum.Ldaps, "LDAPS" );

			  private static readonly IList<VerificationAlgorithm> valueList = new List<VerificationAlgorithm>();

			  static VerificationAlgorithm()
			  {
				  valueList.Add( Https );
				  valueList.Add( Ldaps );
			  }

			  public enum InnerEnum
			  {
				  Https,
				  Ldaps
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal VerificationAlgorithm( string name, InnerEnum innerEnum, string value )
			  {
					this._value = value;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public string Value
			  {
				  get
				  {
						return _value;
				  }
			  }

			 public static IList<VerificationAlgorithm> values()
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

			 public static VerificationAlgorithm valueOf( string name )
			 {
				 foreach ( VerificationAlgorithm enumInstance in VerificationAlgorithm.valueList )
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

}