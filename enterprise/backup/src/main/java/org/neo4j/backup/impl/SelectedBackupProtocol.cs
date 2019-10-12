using System;
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
namespace Org.Neo4j.backup.impl
{

	public sealed class SelectedBackupProtocol
	{
		 public static readonly SelectedBackupProtocol Any = new SelectedBackupProtocol( "Any", InnerEnum.Any, "any" );
		 public static readonly SelectedBackupProtocol Common = new SelectedBackupProtocol( "Common", InnerEnum.Common, "common" );
		 public static readonly SelectedBackupProtocol Catchup = new SelectedBackupProtocol( "Catchup", InnerEnum.Catchup, "catchup" );

		 private static readonly IList<SelectedBackupProtocol> valueList = new List<SelectedBackupProtocol>();

		 static SelectedBackupProtocol()
		 {
			 valueList.Add( Any );
			 valueList.Add( Common );
			 valueList.Add( Catchup );
		 }

		 public enum InnerEnum
		 {
			 Any,
			 Common,
			 Catchup
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 public string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 internal Private readonly;

		 internal SelectedBackupProtocol( string name, InnerEnum innerEnum, string name )
		 {
			  this._name = name;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static SelectedBackupProtocol FromUserInput( string value )
		 {
			  return Stream.of( SelectedBackupProtocol.values() ).filter(proto => value.Equals(proto.name)).findFirst().orElseThrow(() => new Exception(string.Format("Failed to parse `{0}`", value)));
		 }

		public static IList<SelectedBackupProtocol> values()
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

		public static SelectedBackupProtocol valueOf( string name )
		{
			foreach ( SelectedBackupProtocol enumInstance in SelectedBackupProtocol.valueList )
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