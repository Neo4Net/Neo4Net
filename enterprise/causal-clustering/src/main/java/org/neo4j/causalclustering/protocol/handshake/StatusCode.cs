using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.protocol.handshake
{

	/// <summary>
	/// General status codes sent in responses.
	/// </summary>
	public sealed class StatusCode
	{
		 public static readonly StatusCode Success = new StatusCode( "Success", InnerEnum.Success, 0 );
		 public static readonly StatusCode Ongoing = new StatusCode( "Ongoing", InnerEnum.Ongoing, 1 );
		 public static readonly StatusCode Failure = new StatusCode( "Failure", InnerEnum.Failure, -1 );

		 private static readonly IList<StatusCode> valueList = new List<StatusCode>();

		 static StatusCode()
		 {
			 valueList.Add( Success );
			 valueList.Add( Ongoing );
			 valueList.Add( Failure );
		 }

		 public enum InnerEnum
		 {
			 Success,
			 Ongoing,
			 Failure
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private static;

		 internal StatusCode( string name, InnerEnum innerEnum, int codeValue )
		 {
			  this._codeValue = codeValue;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public int CodeValue()
		 {
			  return _codeValue;
		 }

		 public static Optional<StatusCode> FromCodeValue( int codeValue )
		 {
			  IDictionary<int, StatusCode> map = _codeMap.get();
			  if ( map == null )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					 map = Stream.of( StatusCode.values() ).collect(Collectors.toMap(StatusCode::codeValue, System.Func.identity()));

					_codeMap.compareAndSet( null, map );
			  }
			  return Optional.ofNullable( map[codeValue] );
		 }

		public static IList<StatusCode> values()
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

		public static StatusCode valueOf( string name )
		{
			foreach ( StatusCode enumInstance in StatusCode.valueList )
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