////////////using System;
////////////using System.Collections.Generic;

/////////////*
//////////// * Copyright © 2018-2020 "Neo4Net,"
//////////// * Team NeoN [http://neo4net.com]. All Rights Reserved.
//////////// *
//////////// * This file is part of Neo4Net.
//////////// *
//////////// * Neo4Net is free software: you can redistribute it and/or modify
//////////// * it under the terms of the GNU General Public License as published by
//////////// * the Free Software Foundation, either version 3 of the License, or
//////////// * (at your option) any later version.
//////////// *
//////////// * This program is distributed in the hope that it will be useful,
//////////// * but WITHOUT ANY WARRANTY; without even the implied warranty of
//////////// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//////////// * GNU General Public License for more details.
//////////// *
//////////// * You should have received a copy of the GNU General Public License
//////////// * along with this program.  If not, see <http://www.gnu.org/licenses/>.
//////////// */
////////////namespace Neo4Net.GraphDb.config
////////////{

////////////	/// <summary>
////////////	/// All fields specified here are set via annotations when loaded </summary>
////////////	/// @deprecated The settings API will be completely rewritten in 4.0 
////////////	[Obsolete("The settings API will be completely rewritten in 4.0")]
////////////	public abstract class BaseSetting<T> : Setting<T>
////////////	{
////////////		public abstract Optional<System.Func<string, T>> Parser { get; }
////////////		public abstract IList<Setting<T>> Settings( IDictionary<string, string> @params );
////////////		public abstract IDictionary<string, string> Validate( IDictionary<string, string> rawConfig, System.Action<string> warningConsumer );
////////////		public abstract IDictionary<string, T> Values( IDictionary<string, string> validConfig );
////////////		public abstract T From( Configuration config );
////////////		public abstract string DefaultValue { get; }
////////////		public abstract void WithScope( System.Func<string, string> scopingRule );
////////////		public abstract string Name();
////////////		 private bool _deprecated;
////////////		 private string _replacement;
////////////		 private bool @internal;
////////////		 private bool _secret;
////////////		 private bool _dynamic;
////////////		 private string _documentedDefaultValue;
////////////		 private string _description;

////////////		 public override bool Deprecated()
////////////		 {
////////////			  return _deprecated;
////////////		 }

//////////////JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//////////////ORIGINAL LINE: public void setDeprecated(final boolean val)
////////////		 public virtual bool Deprecated
////////////		 {
////////////			 set
////////////			 {
////////////				  _deprecated = value;
////////////			 }
////////////		 }

////////////		 public override Optional<string> Replacement()
////////////		 {
////////////			  return Optional.ofNullable( _replacement );
////////////		 }

//////////////JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//////////////ORIGINAL LINE: public void setReplacement(final String val)
////////////		 public virtual string Replacement
////////////		 {
////////////			 set
////////////			 {
////////////				  _replacement = value;
////////////			 }
////////////		 }

////////////		 public override bool Internal()
////////////		 {
////////////			  return this.Internal;
////////////		 }

//////////////JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//////////////ORIGINAL LINE: public void setInternal(final boolean val)
////////////		 public virtual bool Internal
////////////		 {
////////////			 set
////////////			 {
////////////				  this.Internal = value;
////////////			 }
////////////		 }

////////////		 public override bool Secret()
////////////		 {
////////////			  return _secret;
////////////		 }

//////////////JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//////////////ORIGINAL LINE: public void setSecret(final boolean val)
////////////		 public virtual bool Secret
////////////		 {
////////////			 set
////////////			 {
////////////				  _secret = value;
////////////			 }
////////////		 }

////////////		 public override Optional<string> DocumentedDefaultValue()
////////////		 {
////////////			  return Optional.ofNullable( _documentedDefaultValue );
////////////		 }

//////////////JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//////////////ORIGINAL LINE: public void setDocumentedDefaultValue(final String val)
////////////		 public virtual string DocumentedDefaultValue
////////////		 {
////////////			 set
////////////			 {
////////////				  _documentedDefaultValue = value;
////////////			 }
////////////		 }

////////////		 public override Optional<string> Description()
////////////		 {
////////////			  return Optional.ofNullable( _description );
////////////		 }

//////////////JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//////////////ORIGINAL LINE: public void setDescription(final String description)
////////////		 public virtual string Description
////////////		 {
////////////			 set
////////////			 {
////////////				  _description = value;
////////////			 }
////////////		 }

////////////		 public override string ToString()
////////////		 {
////////////			  return valueDescription();
////////////		 }

////////////		 /// <summary>
////////////		 /// Checks whether this setting is dynamic or not. Dynamic properties are allowed to be changed at runtime without
////////////		 /// restarting the server.
////////////		 /// </summary>
////////////		 /// <returns> {@code true} if this setting can be changed at runtime. </returns>
////////////		 public override bool Dynamic()
////////////		 {
////////////			  return _dynamic;
////////////		 }

////////////		 public virtual bool Dynamic
////////////		 {
////////////			 set
////////////			 {
////////////				  _dynamic = value;
////////////			 }
////////////		 }
////////////	}

////////////}