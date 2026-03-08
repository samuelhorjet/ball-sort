use anchor_lang::prelude::*;

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Copy, Debug, PartialEq, Eq)]
#[repr(u8)]
pub enum PuzzleStatus {
    Initialized = 0,
    BoardReady = 1,
    Started = 2,
    Delegated = 3,
    Solved = 4,
    Finalized = 5,
    Abandoned = 6,
}

impl PuzzleStatus {
    pub fn from_u8(val: u8) -> Option<Self> {
        match val {
            0 => Some(Self::Initialized),
            1 => Some(Self::BoardReady),
            2 => Some(Self::Started),
            3 => Some(Self::Delegated),
            4 => Some(Self::Solved),
            5 => Some(Self::Finalized),
            6 => Some(Self::Abandoned),
            _ => None,
        }
    }
}

impl Default for PuzzleStatus {
    fn default() -> Self {
        Self::Initialized
    }
}
