﻿using System;

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
namespace Org.Neo4j.Kernel.impl.store.record
{
	public abstract class PrimitiveRecord : AbstractBaseRecord
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal long NextPropConflict;

		 internal PrimitiveRecord( long id ) : base( id )
		 {
		 }

		 [Obsolete]
		 internal PrimitiveRecord( long id, long nextProp ) : base( id )
		 {
			  this.NextPropConflict = nextProp;
		 }

		 public override void Clear()
		 {
			  base.Clear();
			  NextPropConflict = Record.NoNextProperty.intValue();
		 }

		 protected internal virtual PrimitiveRecord Initialize( bool inUse, long nextProp )
		 {
			  base.Initialize( inUse );
			  this.NextPropConflict = nextProp;
			  return this;
		 }

		 public virtual long NextProp
		 {
			 get
			 {
				  return NextPropConflict;
			 }
			 set
			 {
				  this.NextPropConflict = value;
			 }
		 }


		 public abstract PropertyRecord IdTo { set; }
	}

}